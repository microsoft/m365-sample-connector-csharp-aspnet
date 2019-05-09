// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// </summary>
    public class FacebookProvider : ConnectorSourceProvider
    {
        /// <summary>
        /// Connector Type
        /// </summary>
        private const string connectorJobType = "Facebook";

        /// <summary>
        /// Client id parameter
        /// </summary>
        private readonly string clientIdParam = "client_id";

        /// <summary>
        /// Client secret parameter
        /// </summary>
        private readonly string clientSecretParam = "client_secret";

        /// <summary>
        /// Redirect Uri parameter
        /// </summary>
        private readonly string redirectUrlParam = "redirect_uri";

        /// <summary>
        /// Code parameter
        /// </summary>
        private readonly string codeParam = "code";

        /// <summary>
        /// Field parameter
        /// </summary>
        protected readonly string fieldsParam = "fields";

        /// <summary>
        /// access token parameter
        /// </summary>
        protected readonly string accessTokenParam = "access_token";

        public IRestApiRepository Client { get; set; }

        public FacebookProvider(AzureTableProvider azureTableProvider)
            :base(azureTableProvider, azureTableProvider.GetAzureTableReference(Settings.TokenTableName))
        {
            Client = new RestApiRepository(SettingsFB.FacebookBaseUrl);
        }

        /// <summary>
        /// Fetch and store user access token from temporary access code
        /// </summary>
        /// <param name="temporaryAccessCode"> Temporary access code</param>
        /// <param name="redirectUrl"> Redirect Url to required by APi</param>
        /// <returns>True if successful</returns>
        public override async Task StoreOAuthToken(string accessCode, string redirectUrl, string jobId)
        {
            string accessToken = await GetUserAccessToken(accessCode, redirectUrl);
            await AddTokenIntoStorage(jobId, accessToken, connectorJobType);
        }

        public async Task<string> GetUserAccessToken(string accessCode, string redirectUrl)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add(this.clientIdParam, SettingsFB.FacebookAppId);
            queryParams.Add(this.clientSecretParam, SettingsFB.FacebookAppSecret);
            queryParams.Add(this.codeParam, accessCode);
            queryParams.Add(this.redirectUrlParam, redirectUrl);
            var accessToken = await this.Client.GetRequestAsync<Dictionary<string, string>>(SettingsFB.FacebookOauthEndpoint, null, queryParams, new CancellationTokenSource().Token);
            return accessToken["access_token"];
        }

        public async Task<string> GetUserEmailId(string userToken)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add(this.accessTokenParam, userToken);
            queryParams.Add(this.fieldsParam, "email");
            Dictionary<string, string> response = await this.Client.GetRequestAsync<Dictionary<string, string>>("me", null, queryParams, new CancellationTokenSource().Token);
            return response["email"];
        }

        /// <summary>
        /// Gets Entities
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <returns>Entities for user</returns>
        public override async Task<IEnumerable<ConnectorEntity>> GetEntities(string jobId)
        {
            string userToken = await GetTokenFromStorage(jobId, connectorJobType);

            if (string.IsNullOrEmpty(userToken))
            {
                return null;
            }

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add(this.accessTokenParam, userToken);
            queryParams.Add(this.fieldsParam, "id,name");
            List<PageFB> pageList = await GatherAllPages(queryParams);
            List<ConnectorEntity> entities = null;
            if (pageList != null && pageList.Count != 0)
            {
                entities = new List<ConnectorEntity>();
                foreach (PageFB page in pageList)
                {
                    entities.Add(new ConnectorEntity()
                    {
                        Id = page.Id,
                        Name = page.Name
                    });
                }
            }
            return entities;
        }

        public async Task<List<PageFB>> GatherAllPages(Dictionary<string, string> queryParams)
        {
            var fullResponse = new List<PageFB>();

            PageListFB response = null;
            do
            {
                response = await this.Client.GetRequestAsync<PageListFB>(SettingsFB.FacebookGraphApiEndpoint, null, queryParams, new CancellationTokenSource().Token);
                if (response == null || response.Data.Length==0)
                {
                    break;
                }
                fullResponse.AddRange(response.Data);
                
                queryParams["after"] = response.Paging.Cursors.After;
            } while (response.Paging?.Next != null);

            return fullResponse;
        }

        /// <summary>
        /// Gets Oauth token for resource
        /// </summary>
        /// <param name="resourceId">Facebook Page Id</param>
        /// <param name="job name">unique job Identifier</param>
        /// <returns>Oauth token for resource</returns>
        public override async Task<string> GetAuthTokenForResource(string resourceId, string jobId)
        {
            string userToken = await GetTokenFromStorage(jobId, connectorJobType);
            string sourceInfo = await GetResourceInfo(userToken, resourceId);
            return sourceInfo;
        }

        public async Task<string> GetResourceInfo(string userToken, string resourceId)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add(this.accessTokenParam, userToken);
            queryParams.Add(this.fieldsParam, "name,access_token");
            PageFB response = await this.Client.GetRequestAsync<PageFB>(resourceId, null, queryParams, new CancellationTokenSource().Token);
            SourceInfoFB sourceInfo = new SourceInfoFB();
            sourceInfo.PageId = response?.Id;
            sourceInfo.PageName = response?.Name;
            sourceInfo.AccessToken = response?.AccessToken;
            return JsonConvert.SerializeObject(sourceInfo);
        }

        /// <summary>
        /// OAuth url for jobType
        /// </summary>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        public override Task<string> GetOAuthUrl(string redirectUrl)
        {
            return Task.FromResult<string>(string.Format("{0}&client_id={1}&redirect_uri={2}",
                                    SettingsFB.FacebookOauthEndpointWithPermissions,
                                    SettingsFB.FacebookAppId,
                                    redirectUrl));
        }
        
        /// <summary>
        /// Subscribe page feed
        /// </summary>
        /// <param name="sourceInfoJson">source info has all the information of the page</param>
        /// <returns>Whether subscribed or not</returns>
        public override async Task<bool> Subscribe(string sourceInfoJson)
        {
            SourceInfoFB sourceInfo = JsonConvert.DeserializeObject<SourceInfoFB>(sourceInfoJson);
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add(this.accessTokenParam, sourceInfo.AccessToken);

            SubscribeWebhookResponseFB response = await this.Client.PostRequestAsync<Dictionary<string, string>, SubscribeWebhookResponseFB>(sourceInfo.PageId + "/subscribed_apps", null, queryParams, new CancellationTokenSource().Token);

            if (response != null && response.Success == true)
            {
                return true;
            }

            Trace.TraceError("Subscription failed for page id {0}", sourceInfo.PageId);
            throw new Exception("Internal Server Error");
        }

        public override async Task<bool> Unsubscribe(string sourceInfoJson)
        {
            SourceInfoFB sourceInfo = JsonConvert.DeserializeObject<SourceInfoFB>(sourceInfoJson);
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add(this.accessTokenParam, sourceInfo.AccessToken);
            try
            {
                SubscribeWebhookResponseFB response = await this.Client.DeleteRequestAsync<SubscribeWebhookResponseFB>(sourceInfo.PageId + "/subscribed_apps", queryParams, new CancellationTokenSource().Token);
                if (response != null && response.Success == true)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Failed to unsubscribe. Exception occured: {e.Message}, {e.StackTrace}");
            }
            
            Trace.TraceError("UnSubscribe failed for page id {0}", sourceInfo.PageId);
            return false;
        }
    }
}