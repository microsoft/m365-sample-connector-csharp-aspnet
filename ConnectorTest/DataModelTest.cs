// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Sample.Connector.FacebookSDK;
    using System.IO;

    [TestClass]
    public class DataModelTest
    {
        [TestMethod]
        public void DataModelTestsFB()
        {
            var response = "{\"id\":\"123\",\"first_name\": \"Micheal\",\"gender\": \"male\",\"last_name\": \"Catala\",\"email\": \"abc.com\",\"link\": \"abcd@user.com\",\"location\": {\"name\":\"India\"},\"verified\": true,\"name\": \"Micheal Catala\",\"picture\": {\"data\": {\"height\": 50,\"url\": \"abc@user.com\",\"width\": 50}}}";
            UserFB user = JsonConvert.DeserializeObject<UserFB>(response);

            response = "{\"id\": \"123\",\"category\": \"Test\",\"checkins\": 0,\"link\": \"abc/123\",\"name\": \"Testing\",\"likes\":3,\"description\":\"Page Desciption\"}";
            UserLocationFB location = JsonConvert.DeserializeObject<UserLocationFB>(response);

            response = "{\"data\":[{\"id\": \"notif_4\",\"from\": {\"name\": \"Micheal Catala\",\"id\": \"1012\"},\"created_time\": \"2018-03-07T07:28:33+0000\",\"updated_time\": \"2018-03-07T07:32:13+0000\",\"title\": \"Micheal Catala and Onkar Singh commented on your post.\",\"link\": \"link\",\"unread\": 1,\"object\": {\"created_time\": \"2018-03-07T07:27:52+0000\",\"message\": \"Running from Job Manager\",\"story\": \"story\",\"id\": \"123\"}}],\"paging\": {\"previous\": \"abc\",},\"summary\": {\"unseen_count\": 2,\"updated_time\": \"2018-04-23T06:03:45+0000\"}}";
            NotificationFB notification = JsonConvert.DeserializeObject<NotificationFB>(response);

            response = "{\"data\":[{\"id\":\"abc\",\"name\":\"abc\",\"type\":\"abc\",\"offset\":1,\"length\":1}],\"paging\": {\"previous\": \"abc\",}}";
            MessageTagsFB tags = JsonConvert.DeserializeObject<MessageTagsFB>(response);

            response = "{\"id\":\"abc\",\"name\":\"abc\",\"access_token\":\"abc\"}";
            PageFB page = JsonConvert.DeserializeObject<PageFB>(response);
        }

        [TestMethod]
        public void DataModelTestFBWebhook()
        {
            // Message Comment by Page
            string data = File.ReadAllText(@"FakeData\FakeWebhookFeedData.json");
            WebhookFeedFB webhook = JsonConvert.DeserializeObject<WebhookFeedFB>(data);
        }
    }
}
