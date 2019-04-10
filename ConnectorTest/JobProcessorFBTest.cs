// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Sample.Connector.FacebookSDK;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    [TestClass]
    public class JobProcessorFBTest
    {
        [TestMethod]
        public void QueryFBTests()
        {
            ConnectorTask taskInfo = new ConnectorTask()
            {
                StartTime = DateTime.Parse("2018-01-01"),
                EndTime = DateTime.Parse("2018-09-17"),
                DirtyEntities = new List<string> { "123", "456" },
            };

            SourceInfoFB sourceInfo = new SourceInfoFB()
            {
                PageId = "123"
            };

            string expectedFeedUrl = "https://graph.facebook.com:443/v3.0/123/feed?fields=id,created_time,from{name,id,picture},to,message,story,likes.summary(true),reactions.summary(true),comments{from{name,id,picture},id,created_time,message,parent{id},comment_count,like_count,attachment,message_tags,comments{from{name,id,picture},id,created_time,message,parent{id},comment_count,like_count,attachment,message_tags}},attachments,source,message_tags,type,status_type&since=2018-01-01T00:00:00&until=2018-09-17T00:00:00";
            string actualFeedUrl = QueryFB.GetFeedUrl(taskInfo, sourceInfo);
            Assert.AreEqual(expectedFeedUrl, actualFeedUrl);

            string expectedUpdatePostsUrl = "https://graph.facebook.com:443/v3.0/?ids=123,456&fields=id,created_time,from{name,id,picture},to,message,story,likes.summary(true),reactions.summary(true),comments{from{name,id,picture},id,created_time,message,parent{id},comment_count,like_count,attachment,message_tags,comments{from{name,id,picture},id,created_time,message,parent{id},comment_count,like_count,attachment,message_tags}},attachments,source,message_tags,type,status_type";
            string actualUpdatePostsUrl = QueryFB.GetUpdatedPostsUrl(taskInfo);
            Assert.AreEqual(expectedUpdatePostsUrl, actualUpdatePostsUrl);

            string expectedVideoSourceUrl = "https://graph.facebook.com:443/v3.0/123?fields=source";
            string actualVideoSourceUrl = QueryFB.GetVideoUrl("123");
            Assert.AreEqual(expectedVideoSourceUrl, actualVideoSourceUrl);
        }

        [TestMethod]
        public async Task DownloadDataAndTransformUnitTest()
        {
            ConnectorTask taskInfo = new ConnectorTask();
            taskInfo.StartTime = DateTime.Parse("2018-01-09");
            taskInfo.EndTime = DateTime.Parse("2018-01-11");
            taskInfo.JobId = "job1";
            taskInfo.TaskId = "task1";
            taskInfo.TenantId = "tenant1";
            
            var mockDownloader = new Mock<IDownloader>();
            mockDownloader.SetupSequence(x => x.GetWebContent<PostListFB, ErrorsFB>(It.IsAny<string>(), It.IsAny<AuthenticationHeaderValue>()))
                .ReturnsAsync(JsonConvert.DeserializeObject<PostListFB>(File.ReadAllText(@"FakeData\FakeData.json")))
                .ReturnsAsync(JsonConvert.DeserializeObject<PostListFB>("{\"data\": []}"));
            
            mockDownloader.Setup(x => x.DownloadFileAsBase64EncodedString(It.IsAny<string>()))
                .ReturnsAsync(Convert.ToBase64String(File.ReadAllBytes(@"FakeData\FakeImage.jpg")));

            FakeUploader uploader = new FakeUploader();
            JobProcessorFB job = new JobProcessorFB(mockDownloader.Object, uploader);

            string sourceInfo = "{\"PageId\":\"123\",\"AccessToken\":\"Fake\",\"PageName\":\"Fake123\"}";

            await job.FetchData(taskInfo, sourceInfo);

            // Assert 1 item of each type - post, comment, reply
            int assertItemsCount = 0;
            foreach (var entry in uploader.fakeStorage)
            {
                if (File.Exists($@"FakeData\{entry.Key}.json"))
                {
                    assertItemsCount++;
                    string expectedJson = File.ReadAllText($@"FakeData\{entry.Key}.json");
                    Item expectedItem = JsonConvert.DeserializeObject<Item>(expectedJson);
                    AssertItemsAreEqual(expectedItem, entry.Value);
                }
            }
            Assert.AreEqual(assertItemsCount, 3);
        }

        [TestMethod]
        public async Task ErrorsFBUnitTest()
        {
            ConnectorTask taskInfo = new ConnectorTask();
            taskInfo.StartTime = DateTime.Parse("2018-01-09");
            taskInfo.EndTime = DateTime.Parse("2018-01-11");
            taskInfo.JobId = "job1";
            taskInfo.TaskId = "task1";
            taskInfo.TenantId = "tenant1";

            var mockDownloader = new Mock<IDownloader>();
            mockDownloader.Setup(x => x.GetWebContent<PostListFB, ErrorsFB>(It.IsAny<string>(), It.IsAny<AuthenticationHeaderValue>()))
                .Throws(new ClientException<ErrorsFB>(JsonConvert.DeserializeObject<ErrorsFB>(File.ReadAllText(@"FakeData\FakeError.json"))));

            FakeUploader uploader = new FakeUploader();
            JobProcessorFB job = new JobProcessorFB(mockDownloader.Object, uploader);

            string sourceInfo = "{\"PageId\":\"123\",\"AccessToken\":\"Fake\",\"PageName\":\"Fake123\"}";

            try
            {
                await job.FetchData(taskInfo, sourceInfo);
            }
            catch (ClientException<ErrorsFB> error)
            {
                Assert.AreEqual(error.error.Error.ErrorMessage, "Message describing the error");
                Assert.AreEqual(error.error.Error.ErrorType, "OAuthException");
            }

            Assert.AreEqual(uploader.fakeStorage.Count, 0);
        }

        [TestMethod]
        public async Task NoPostsFetchedUnitTest()
        {
            ConnectorTask taskInfo = new ConnectorTask();
            taskInfo.StartTime = DateTime.Parse("2018-01-09");
            taskInfo.EndTime = DateTime.Parse("2018-01-11");
            taskInfo.JobId = "job1";
            taskInfo.TaskId = "task1";
            taskInfo.TenantId = "tenant1";

            var mockDownloader = new Mock<IDownloader>();
            mockDownloader.Setup(x => x.GetWebContent<PostListFB, ErrorsFB>(It.IsAny<string>(), It.IsAny<AuthenticationHeaderValue>()))
                .ReturnsAsync(JsonConvert.DeserializeObject<PostListFB>("{\"data\": []}"));

            FakeUploader uploader = new FakeUploader();
            JobProcessorFB job = new JobProcessorFB(mockDownloader.Object, uploader);

            string sourceInfo = "{\"PageId\":\"123\",\"AccessToken\":\"Fake\",\"PageName\":\"Fake123\"}";

            await job.FetchData(taskInfo, sourceInfo);
            Assert.AreEqual(uploader.fakeStorage.Count, 0);
        }

        [TestMethod]
        public async Task FetchUpdatedPostsUnitTest()
        {
            ConnectorTask taskInfo = new ConnectorTask();
            taskInfo.StartTime = DateTime.Parse("2018-01-09");
            taskInfo.EndTime = DateTime.Parse("2018-01-11");
            taskInfo.JobId = "job1";
            taskInfo.TaskId = "task1";
            taskInfo.TenantId = "tenant1";
            taskInfo.DirtyEntities = new List<string>()
            {
                "a",
                "b"
            };

            var mockDownloader = new Mock<IDownloader>();
            mockDownloader.SetupSequence(x => x.GetWebContent<PostListFB, ErrorsFB>(It.IsAny<string>(), It.IsAny<AuthenticationHeaderValue>()))
                .ReturnsAsync(JsonConvert.DeserializeObject<PostListFB>("{\"data\": []}"));

            mockDownloader.SetupSequence(x => x.GetWebContent<Dictionary<string, PostFB>, ErrorsFB>(It.IsAny<string>(), It.IsAny<AuthenticationHeaderValue>()))
                .ReturnsAsync(JsonConvert.DeserializeObject<Dictionary<string, PostFB>>(File.ReadAllText(@"FakeData\FakeDirtyPosts.json")));

            FakeUploader uploader = new FakeUploader();
            JobProcessorFB job = new JobProcessorFB(mockDownloader.Object, uploader);

            string sourceInfo = "{\"PageId\":\"123\",\"AccessToken\":\"Fake\",\"PageName\":\"Fake123\"}";

            await job.FetchData(taskInfo, sourceInfo);
            Assert.AreEqual(uploader.fakeStorage.Count, 2);
        }

        private void AssertItemsAreEqual(Object e, Object a)
        {
            string x = JsonConvert.SerializeObject(e);
            string y = JsonConvert.SerializeObject(a);
            JObject eObj = JObject.Parse(JsonConvert.SerializeObject(e));
            JObject aObj = JObject.Parse(JsonConvert.SerializeObject(a));
            Assert.IsTrue(JToken.DeepEquals(eObj, aObj));
        }
    }
}
