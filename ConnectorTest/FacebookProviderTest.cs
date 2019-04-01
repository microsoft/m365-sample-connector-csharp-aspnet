// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Sample.Connector.FacebookSDK;

    [TestClass]
    public class FacebookProviderTest
    {
        [TestMethod]
        public void FBOAuthUrlTest()
        {
            FacebookProvider provider = new FacebookProvider();
            string url = provider.GetOAuthUrl("FAKE_REDIRECT_URL").Result;
            Assert.AreEqual(url,
                "https://www.facebook.com/v3.0/dialog/oauth?scope=manage_pages,pages_show_list,email&client_id=&redirect_uri=FAKE_REDIRECT_URL");
        }

        [TestMethod]
        public async Task TestGatherAllPages()
        {
            var responsePart1 = new PageListFB
            {
                Data = new[]
                {
                    new PageFB
                    {
                        Id = "1",
                        Name = "p1"
                    }
                },
                Paging = new PagingFB
                {
                    Cursors = new CursorsFB
                    {
                        Before = "NA",
                        After = "first"
                    },
                    Next = "RANDOM"
                }
            };

            var responseLastPart = new PageListFB
            {
                Data = new[]
                {
                    new PageFB
                    {
                        Id = "2",
                        Name = "p2"
                    }
                },
                Paging = new PagingFB
                {
                    Cursors = new CursorsFB
                    {
                        Before = "NA",
                        After = "first"
                    },
                    Next = null
                }
            };
            var restApiRepositoryMock = new Mock<IRestApiRepository>();
            restApiRepositoryMock.SetupSequence(x => x.GetRequestAsync<PageListFB>(It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responsePart1)
                .ReturnsAsync(responseLastPart);
            var facebookProvider = new FacebookProvider
            {
                Client = restApiRepositoryMock.Object
            };
            var queryParams = new Dictionary<string, string> {{"access_token", "any"}, {"fields", "id,name"}};
            var allPages = await facebookProvider.GatherAllPages(queryParams);
            Assert.AreEqual(responsePart1.Data[0], allPages[0]);
            Assert.AreEqual(responseLastPart.Data[0], allPages[1]);
        }

        [TestMethod]
        public async Task TestNoPagesReturned()
        {
            
            var emptyResponse = new PageListFB
            {
                Data = new PageFB[] { }
            };

            var restApiRepositoryMock = new Mock<IRestApiRepository>();
            restApiRepositoryMock.SetupSequence(x => x.GetRequestAsync<PageListFB>(It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyResponse);
            var facebookProvider = new FacebookProvider
            {
                Client = restApiRepositoryMock.Object
            };
            var queryParams = new Dictionary<string, string> {{"access_token", "any"}, {"fields", "id,name"}};
            var allPages = await facebookProvider.GatherAllPages(queryParams);
            Assert.AreEqual(0,allPages.Count);
        }

        [TestMethod]
        public async Task GetUserAccessTokenTest()
        {
            var response = new Dictionary<string, string>
            {
                { "access_token", "any" }
            };

            var restApiRepositoryMock = new Mock<IRestApiRepository>();
            restApiRepositoryMock.SetupSequence(x => x.GetRequestAsync<Dictionary<string, string>>(It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var facebookProvider = new FacebookProvider
            {
                Client = restApiRepositoryMock.Object
            };
            var accessToken = await facebookProvider.GetUserAccessToken(string.Empty, string.Empty);
            Assert.AreEqual(accessToken, "any");
        }

        [TestMethod]
        public async Task GetUserEmailIdTest()
        {
            var response = new Dictionary<string, string>
            {
                { "email", "any" }
            };

            var restApiRepositoryMock = new Mock<IRestApiRepository>();
            restApiRepositoryMock.SetupSequence(x => x.GetRequestAsync<Dictionary<string, string>>(It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var facebookProvider = new FacebookProvider
            {
                Client = restApiRepositoryMock.Object
            };
            var emailId = await facebookProvider.GetUserEmailId(string.Empty);
            Assert.AreEqual(emailId, "any");
        }

        [TestMethod]
        public async Task GetResourceInfoTest()
        {
            var response = new PageFB()
            {
                Id = "id",
                Name = "name",
                AccessToken = "accessToken"
            };

            var restApiRepositoryMock = new Mock<IRestApiRepository>();
            restApiRepositoryMock.SetupSequence(x => x.GetRequestAsync<PageFB>(It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var facebookProvider = new FacebookProvider
            {
                Client = restApiRepositoryMock.Object
            };
            var sourceInfo = await facebookProvider.GetResourceInfo(string.Empty, string.Empty);
            SourceInfoFB info = JsonConvert.DeserializeObject<SourceInfoFB>(sourceInfo);
            Assert.AreEqual(info.PageId, "id");
            Assert.AreEqual(info.PageName, "name");
            Assert.AreEqual(info.AccessToken, "accessToken");
        }

        [TestMethod]
        public async Task WebhookSubscribeSuccessTest()
        {
            string sourceInfo = "{\"PageId\":\"abc\",\"AccessToken\":\"abc\",\"PageName\":\"abc\"}";
            SubscribeWebhookResponseFB response = new SubscribeWebhookResponseFB() {
                Success = true,
            };
            var restApiRepositoryMock = new Mock<IRestApiRepository>();
            restApiRepositoryMock.SetupSequence(x => x.PostRequestAsync<Dictionary<string, string>, SubscribeWebhookResponseFB>(It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var facebookProvider = new FacebookProvider
            {
                Client = restApiRepositoryMock.Object
            };
            var subscribed = await facebookProvider.Subscribe(sourceInfo);
            Assert.AreEqual(subscribed, true);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Webhook Subscription threw exception")]
        public async Task WebhookSubscribeFailureTest()
        {
            string sourceInfo = "{\"PageId\":\"abc\",\"AccessToken\":\"abc\",\"PageName\":\"abc\"}";
            SubscribeWebhookResponseFB response = new SubscribeWebhookResponseFB()
            {
                Success = false,
            };
            var restApiRepositoryMock = new Mock<IRestApiRepository>();
            restApiRepositoryMock.SetupSequence(x => x.PostRequestAsync<Dictionary<string, string>, SubscribeWebhookResponseFB>(It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var facebookProvider = new FacebookProvider
            {
                Client = restApiRepositoryMock.Object
            };
            var subscribed = await facebookProvider.Subscribe(sourceInfo);
        }

        [TestMethod]
        public async Task WebhookUnsubscribeSuccessTest()
        {
            string sourceInfo = "{\"PageId\":\"abc\",\"AccessToken\":\"abc\",\"PageName\":\"abc\"}";
            SubscribeWebhookResponseFB response = new SubscribeWebhookResponseFB()
            {
                Success = true,
            };
            var restApiRepositoryMock = new Mock<IRestApiRepository>();
            restApiRepositoryMock.SetupSequence(x => x.DeleteRequestAsync<SubscribeWebhookResponseFB>(It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var facebookProvider = new FacebookProvider
            {
                Client = restApiRepositoryMock.Object
            };
            var unsubscribed = await facebookProvider.Unsubscribe(sourceInfo);
            Assert.AreEqual(unsubscribed, true);
        }

        [TestMethod]
        public async Task WebhookUnsubscribeFailureTest()
        {
            string sourceInfo = "{\"PageId\":\"abc\",\"AccessToken\":\"abc\",\"PageName\":\"abc\"}";
            SubscribeWebhookResponseFB response = new SubscribeWebhookResponseFB()
            {
                Success = false,
            };
            var restApiRepositoryMock = new Mock<IRestApiRepository>();
            restApiRepositoryMock.SetupSequence(x => x.DeleteRequestAsync<SubscribeWebhookResponseFB>(It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var facebookProvider = new FacebookProvider
            {
                Client = restApiRepositoryMock.Object
            };
            var unsubscribed = await facebookProvider.Unsubscribe(sourceInfo);
            Assert.AreEqual(unsubscribed, false);
        }
    }
}