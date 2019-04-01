// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Sample.Connector.FacebookSDK;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
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
                PageId = "1131404197001700"
            };

            string expectedFeedUrl = "https://graph.facebook.com:443/v3.0/1131404197001700/feed?fields=id,created_time,from{name,id,picture},to,message,story,likes.summary(true),reactions.summary(true),comments{from{name,id,picture},id,created_time,message,parent{id},comment_count,like_count,attachment,message_tags,comments{from{name,id,picture},id,created_time,message,parent{id},comment_count,like_count,attachment,message_tags}},attachments,source,message_tags,type,status_type&since=2018-01-01T00:00:00&until=2018-09-17T00:00:00";
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
            taskInfo.JobId = "12813d8d-f3d3-4c50-b1f0-56d23e9430f4";
            taskInfo.TaskId = "78616d8d-f2d3-4350-b3f0-46d23e5430f4";
            taskInfo.TenantId = "39238e87-b5ab-4ef6-a559-af54c6b07b42";

            FakeDownloader downloader = new FakeDownloader(TestCases.DummyPosts);

            FakeUploader uploader = new FakeUploader();
            JobProcessorFB job = new JobProcessorFB(downloader, uploader);
            
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
            taskInfo.JobId = "12813d8d-f3d3-4c50-b1f0-56d23e9430f4";
            taskInfo.TaskId = "78616d8d-f2d3-4350-b3f0-46d23e5430f4";
            taskInfo.TenantId = "39238e87-b5ab-4ef6-a559-af54c6b07b42";

            FakeDownloader downloader = new FakeDownloader(TestCases.DummyError);

            FakeUploader uploader = new FakeUploader();
            JobProcessorFB job = new JobProcessorFB(downloader, uploader);

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
            taskInfo.JobId = "12813d8d-f3d3-4c50-b1f0-56d23e9430f4";
            taskInfo.TaskId = "78616d8d-f2d3-4350-b3f0-46d23e5430f4";
            taskInfo.TenantId = "39238e87-b5ab-4ef6-a559-af54c6b07b42";

            FakeDownloader downloader = new FakeDownloader(TestCases.NoPostsFetched);

            FakeUploader uploader = new FakeUploader();
            JobProcessorFB job = new JobProcessorFB(downloader, uploader);
            
            string sourceInfo = "{\"PageId\":\"123\",\"AccessToken\":\"Fake\",\"PageName\":\"Fake123\"}";

            await job.FetchData(taskInfo, sourceInfo);
            Assert.AreEqual(uploader.fakeStorage.Count, 0);
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
