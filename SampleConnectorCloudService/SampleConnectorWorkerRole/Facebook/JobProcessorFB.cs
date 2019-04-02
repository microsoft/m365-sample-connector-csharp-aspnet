// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using FacebookSDK;

    public class JobProcessorFB : JobProcessorBase
    {
        /// <summary>
        /// For making Http requests
        /// </summary>
        private IDownloader downloader;
        
        private readonly IUploader uploader;

        /// <summary>
        /// Constructor
        /// </summary>
        public JobProcessorFB(IDownloader downloader, IUploader uploader)
        {
            this.downloader = downloader;
            this.uploader = uploader;
        }

        /// <summary>
        /// Fetches Data wrt given time interval
        /// </summary>
        /// <param name="taskInfo">contains the time stamps for which data is to be fetched</param>
        /// <param name="sourceInfo">source Info</param>
        public override async Task<List<ItemMetadata>> FetchData(ConnectorTask taskInfo, string sourceInfo)
        {
            SourceInfoFB fbSourceInfo = JsonConvert.DeserializeObject<SourceInfoFB>(sourceInfo);
            List<ItemMetadata> itemMetadata = new List<ItemMetadata>();
            await FetchPosts(taskInfo, fbSourceInfo, itemMetadata);
            await FetchUpdates(taskInfo, fbSourceInfo, itemMetadata);
            return itemMetadata;
        }

        /// <summary>
        /// Fetches Posts wrt given time interval
        /// </summary>
        /// <param name="taskInfo">contains the time stamps for which data is to be fetched</param>
        /// <param name="sourceInfo">contains data source information</param>
        public async Task FetchPosts(ConnectorTask taskInfo, SourceInfoFB sourceInfo, List<ItemMetadata> itemMetadata)
        {
            string url = QueryFB.GetFeedUrl(taskInfo, sourceInfo);
            Trace.TraceInformation($"Fetching Data from Facebook, TenantId: {taskInfo.TenantId}, JobId: {taskInfo.JobId}, StartTime: {taskInfo.StartTime.ToString()}, EndTime: {taskInfo.EndTime.ToString()}");
            AuthenticationHeaderValue header = new AuthenticationHeaderValue("Bearer", sourceInfo.AccessToken);

            do
            {
                PostListFB list = await this.downloader.GetWebContent<PostListFB, ErrorsFB>(url, header);
                PostFB[] postList = list.Data;
                PagingFB pagingPointers = list.Paging;
                if (postList.Count() == 0)
                {
                    break;
                }
                foreach (PostFB post in postList)
                {
                    await HandlePost(post, header, sourceInfo.PageId, sourceInfo.PageName, taskInfo, itemMetadata);
                }
                url = pagingPointers?.Next;
            } while (url != null);
        }

        /// <summary>
        /// Fetches updated posts
        /// </summary>
        /// <param name="taskInfo">info related to task, eg accessToken</param>
        /// <param name="sourceInfo">contains data source information</param>
        public async Task FetchUpdates(ConnectorTask taskInfo, SourceInfoFB sourceInfo, List<ItemMetadata> itemMetadata)
        {
            if (taskInfo.DirtyEntities != null && taskInfo.DirtyEntities?.Count != 0)
            {
                Trace.TraceInformation($"Number of dirty posts: {taskInfo.DirtyEntities?.Count}");

                string url = QueryFB.GetUpdatedPostsUrl(taskInfo);

                AuthenticationHeaderValue header = new AuthenticationHeaderValue("Bearer", sourceInfo.AccessToken);
                Dictionary<string, PostFB> dict = await this.downloader.GetWebContent<Dictionary<string, PostFB>, ErrorsFB>(url, header);
                foreach (KeyValuePair<string, PostFB> postEntry in dict)
                {
                    await HandlePost(postEntry.Value, header, sourceInfo.PageId, sourceInfo.PageName, taskInfo, itemMetadata);
                }
            }
        }

        private async Task HandlePost(PostFB post, AuthenticationHeaderValue header, string pageId, string pageName, ConnectorTask taskInfo, List<ItemMetadata> itemMetadata)
        {
            Item postItem = await CreatePostItem(post, pageId, pageName, taskInfo, itemMetadata);

            CommentFB comments = post.Comments;
            bool moreComments = false;

            do
            {
                if (moreComments)
                {
                    // only if there are more comments to this post, to be fetched
                    comments = await this.downloader.GetWebContent<CommentFB, ErrorsFB>(comments.Paging.Next, header);
                }

                if (comments != null && comments.Data.Count() != 0)
                {
                    List<CommentDataFB> Data = comments.Data.ToList();
                    foreach (CommentDataFB comment in Data)
                    {
                        await HandleComment(post, comment, header, pageId, postItem, taskInfo, itemMetadata);
                    }
                }
                moreComments = true;
            } while (comments?.Paging?.Next != null);
        }

        private async Task HandleComment(PostFB post, CommentDataFB comment, AuthenticationHeaderValue header, string pageId, Item postItem, ConnectorTask taskInfo, List<ItemMetadata> itemMetadata)
        {
            Item commentItem = await CreateCommentItem(comment, pageId, postItem, taskInfo, itemMetadata);

            if (comment.CommentCount > 0)
            {
                bool moreReplies = false;

                do
                {
                    if (moreReplies)
                    {
                        // only if there are more replies to this comment, to be fetched
                        comment.Comments = await this.downloader.GetWebContent<CommentFB, ErrorsFB>(comment.Comments.Paging.Next, header);
                    }
                    if (comment.Comments?.Data != null)
                    {
                        foreach (CommentDataFB reply in comment.Comments.Data)
                        {
                            await HandleReply(reply, pageId, commentItem, postItem, taskInfo, itemMetadata);
                        }
                        moreReplies = true;
                    }
                } while (comment.Comments?.Paging?.Next != null);
            }
        }

        private async Task HandleReply(CommentDataFB reply, string pageId, Item commentItem, Item postItem, ConnectorTask taskInfo, List<ItemMetadata> itemMetadata)
        {
            await CreateReplyItem(reply, pageId, commentItem, postItem, taskInfo, itemMetadata);
        }

        private async Task<Item> CreatePostItem(PostFB post, string pageId, string pageName, ConnectorTask taskInfo, List<ItemMetadata> itemMetadata)
        {
            Item postItem = new Item()
            {
                SchemaVersion = new Version(1, 0),
                Id = post.Id,
                ContainerId = pageId,
                ContainerName = pageName,
                SourceType = "Facebook",
                ItemType = "Post",
                ContentType = ContentType.Text,
                Content = post.Message,
                ParentId = string.Empty,
                ThreadId = post.Id,
                SentTimeUtc = DateTime.Parse(post.CreatedTime),
                Sender = ToItemUser(post.From),
                NumOfLikes = post.Likes?.Summary?.TotalCount ?? 0,
                MessagePreviewText = post.Message,
                Recipients = Array.Empty<User>(),
            };

            if (post.Attachments != null)
            {
                postItem.ContentAttachments = new List<ContentAttachment>();
                if (post.Attachments.Data?[0]?.Media == null)
                {
                    AttachmentDataFB[] attachmentData = post.Attachments.Data?[0]?.Subattachments?.Data;
                    foreach (AttachmentDataFB attachmentItem in attachmentData)
                    {
                        string downloadedContent = await this.downloader.DownloadFileAsBase64EncodedString(attachmentItem.Media?.Image?.Src);

                        ContentAttachment attachment = new ContentAttachment()
                        {
                            AttachmentType = attachmentItem.Type,
                            Content = downloadedContent,
                            Uri = new Uri(attachmentItem.Media?.Image?.Src),
                        };

                        postItem.ContentAttachments.Add(attachment);
                    }
                }
                else
                {
                    // only one video allowed per post, checking attachment type
                    string attachmentType = post.Attachments.Data[0].Type;
                    string downloadedContent = await this.downloader.DownloadFileAsBase64EncodedString(post.Attachments.Data[0].Media?.Image?.Src);

                    ContentAttachment attachment = new ContentAttachment()
                    {
                        AttachmentType = attachmentType,
                        Content = downloadedContent,
                        Uri = new Uri(attachmentType.Contains("video") ? post.Attachments.Data[0].Url : post.Attachments.Data[0].Media?.Image?.Src),
                    };

                    postItem.ContentAttachments.Add(attachment);
                }
            }

            string fileName = await uploader.UploadItem(taskInfo.JobId, taskInfo.TaskId, postItem);
            itemMetadata.Add(new ItemMetadata(postItem.Id, postItem.SentTimeUtc, fileName));
            return postItem;
        }

        private async Task<Item> CreateCommentItem(CommentDataFB comment, string pageId, Item postItem, ConnectorTask taskInfo, List<ItemMetadata> itemMetadata)
        {
            Item commentItem = new Item()
            {
                SchemaVersion = new Version(1, 0),
                Id = comment.Id,
                ContainerId = pageId,
                ContainerName = postItem.ContainerName,
                SourceType = "Facebook",
                ItemType = "Comment",
                ContentType = ContentType.Text,
                Content = comment.Message,
                ParentId = postItem.Id,
                ThreadId = postItem.Id,
                SentTimeUtc = DateTime.Parse(comment.CreatedTime),
                Sender = ToItemUser(comment.From),
                NumOfLikes = comment.LikeCount,
                MessagePreviewText = postItem.Content,
                Recipients = Array.Empty<User>(),
            };

            if (comment.Attachment != null)
            {
                commentItem.ContentAttachments = new List<ContentAttachment>();
                string attachmentType = comment.Attachment.Type;
                string downloadedContent = await this.downloader.DownloadFileAsBase64EncodedString(comment.Attachment.Media?.Image?.Src);

                ContentAttachment attachment = new ContentAttachment()
                {
                    AttachmentType = attachmentType,
                    Content = downloadedContent,
                    Uri = new Uri(attachmentType.Contains("video") ? comment.Attachment.Url : comment.Attachment.Media?.Image?.Src),
                };

                commentItem.ContentAttachments.Add(attachment);
            }

            string fileName = await uploader.UploadItem(taskInfo.JobId, taskInfo.TaskId, commentItem);
            itemMetadata.Add(new ItemMetadata(commentItem.Id, commentItem.SentTimeUtc, fileName));
            return commentItem;
        }

        private async Task CreateReplyItem(CommentDataFB reply, string pageId, Item commentItem, Item postItem, ConnectorTask taskInfo, List<ItemMetadata> itemMetadata)
        {
            Item replyItem = new Item()
            {
                SchemaVersion = new Version(1, 0),
                Id = reply.Id,
                ContainerId = pageId,
                ContainerName = postItem.ContainerName,
                SourceType = "Facebook",
                ItemType = "Reply",
                ContentType = ContentType.Text,
                Content = reply.Message,
                ParentId = commentItem.Id,
                ThreadId = postItem.Id,
                SentTimeUtc = DateTime.Parse(reply.CreatedTime),
                Sender = ToItemUser(reply.From),
                NumOfLikes = reply.LikeCount,
                MessagePreviewText = postItem.Content,
                Recipients = Array.Empty<User>(),
            };

            if (reply.Attachment != null)
            {
                replyItem.ContentAttachments = new List<ContentAttachment>();
                string attachmentType = reply.Attachment.Type;
                string downloadedContent = await this.downloader.DownloadFileAsBase64EncodedString(reply.Attachment.Media?.Image?.Src);

                ContentAttachment attachment = new ContentAttachment()
                {
                    AttachmentType = attachmentType,
                    Content = downloadedContent,
                    Uri = new Uri(attachmentType.Contains("video") ? reply.Attachment.Url : reply.Attachment.Media?.Image?.Src),
                };

                replyItem.ContentAttachments.Add(attachment);
            }

            string fileName = await uploader.UploadItem(taskInfo.JobId, taskInfo.TaskId, replyItem);
            itemMetadata.Add(new ItemMetadata(replyItem.Id, replyItem.SentTimeUtc, fileName));
        }

        private static User ToItemUser(UserFB user)
        {
            return new User
            {
                Id = user?.Id,
                UserProfilePhotoUrl = user?.Picture?.Data?.Url ?? "",
                Name = user?.Name ?? "Anonymous",
                EmailAddress = user?.Id ?? "anonymous"
            };
        }
    }

    /// <summary>
    /// Class for making Facebook Queries
    /// </summary>
    public class QueryFB
    {
        public static string GetFeedUrl(ConnectorTask taskInfo, SourceInfoFB sourceInfo)
        {
            UriBuilder uriBuilder = new UriBuilder(Settings.FacebookBaseUrl);
            uriBuilder.Path += $"/{sourceInfo.PageId}/feed";
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["fields"] = Settings.FacebookQueryFields;
            query["since"] = taskInfo.StartTime.ToString("yyyy-MM-ddTHH:mm:ss");
            query["until"] = taskInfo.EndTime.ToString("yyyy-MM-ddTHH:mm:ss");
            uriBuilder.Query = query.ToString();
            return HttpUtility.UrlDecode(uriBuilder.ToString());
        }

        public static string GetUpdatedPostsUrl(ConnectorTask taskInfo)
        {
            UriBuilder uriBuilder = new UriBuilder(Settings.FacebookBaseUrl);
            uriBuilder.Path += $"/";
            string ids = string.Join(",", taskInfo.DirtyEntities);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["ids"] = ids;
            query["fields"] = Settings.FacebookQueryFields;
            uriBuilder.Query = query.ToString();
            return HttpUtility.UrlDecode(uriBuilder.ToString());
        }

        public static string GetVideoUrl(string videoId)
        {
            UriBuilder uriBuilder = new UriBuilder(Settings.FacebookBaseUrl);
            uriBuilder.Path += $"/{videoId}";
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["fields"] = "source";
            uriBuilder.Query = query.ToString();
            return HttpUtility.UrlDecode(uriBuilder.ToString());
        }
    }
}