// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Threading;
    using Sample.Connector.FacebookSDK;

    /// <summary>
    /// </summary>
    public class FacebookProvider : IConnectorSourceProvider
    {
        private readonly AzureTableProvider azureTableProvider;

        private CloudTable tokenTable;

        /// <summary>
        /// Authentication resource API 
        /// </summary>
        private readonly string authEndpointResource;

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
        /// Pages list resource API 
        /// </summary>
        protected readonly string graphApiResourceName;

        /// <summary>
        /// Field parameter
        /// </summary>
        protected readonly string fieldsParam = "fields";

        /// <summary>
        /// access token parameter
        /// </summary>
        protected readonly string accessTokenParam = "access_token";

        public IRestApiRepository Client { get; set; }

        public FacebookProvider()
        {
            this.graphApiResourceName = Settings.FacebookGraphApiEndpoint;
            this.authEndpointResource = Settings.FacebookOauthEndpoint;
            this.azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            Client = new RestApiRepository(Settings.FacebookBaseUrl);
        }

        /// <summary>
        /// Fetch and store user access token from temporary access code
        /// </summary>
        /// <param name="temporaryAccessCode"> Temporary access code</param>
        /// <param name="redirectUrl"> Redirect Url to required by APi</param>
        /// <returns>True if successful</returns>
        public async Task StoreOAuthToken(string accessCode, string redirectUrl, string jobId)
        {
            string accessToken = await GetUserAccessToken(accessCode, redirectUrl);
            await AddTokenIntoStorage(jobId, accessToken);
        }

        public async Task<string> GetUserAccessToken(string accessCode, string redirectUrl)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add(this.clientIdParam, Settings.FacebookAppId);
            queryParams.Add(this.clientSecretParam, Settings.FacebookAppSecret);
            queryParams.Add(this.codeParam, accessCode);
            queryParams.Add(this.redirectUrlParam, redirectUrl);
            var accessToken = await this.Client.GetRequestAsync<Dictionary<string, string>>(this.authEndpointResource, queryParams, new CancellationTokenSource().Token);
            return accessToken["access_token"];
        }

        private async Task AddTokenIntoStorage(string jobId, string accessToken)
        {
            string accessTokenKey = makeAccessTokenKey(jobId);
            tokenTable = azureTableProvider.GetAzureTableReference(Settings.TokenTableName);
            TokenTableEntity entity = new TokenTableEntity(accessTokenKey, ConnectorJobType.Facebook.ToString());
            entity.Token = accessToken;
            await azureTableProvider.InsertOrReplaceEntityAsync(tokenTable, entity);
        }

        /// <summary>
        /// Get email id used to Login to Facebook
        /// </summary>
        /// <returns>Email id used to login to facebook</returns>
        public async Task<string> GetEmailId(string jobId)
        {
            string userToken = await GetTokenFromStorage(jobId);
            string emailId = await GetUserEmailId(userToken);
            return emailId; 
        }

        public async Task<string> GetUserEmailId(string userToken)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add(this.accessTokenParam, userToken);
            queryParams.Add(this.fieldsParam, "email");
            Dictionary<string, string> response = await this.Client.GetRequestAsync<Dictionary<string, string>>("me", queryParams, new CancellationTokenSource().Token);
            return response["email"];
        }

        /// <summary>
        /// Gets Entities
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <returns>Entities for user</returns>
        public virtual async Task<IEnumerable<ConnectorEntity>> GetEntities(string jobId)
        {
            string userToken = await GetTokenFromStorage(jobId);

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
                response = await this.Client.GetRequestAsync<PageListFB>(this.graphApiResourceName, queryParams, new CancellationTokenSource().Token);
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
        public virtual async Task<string> GetAuthTokenForResource(string resourceId, string jobId)
        {
            string userToken = await GetTokenFromStorage(jobId);
            string sourceInfo = await GetResourceInfo(userToken, resourceId);
            return sourceInfo;
        }

        public async Task<string> GetResourceInfo(string userToken, string resourceId)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add(this.accessTokenParam, userToken);
            queryParams.Add(this.fieldsParam, "name,access_token");
            PageFB response = await this.Client.GetRequestAsync<PageFB>(resourceId, queryParams, new CancellationTokenSource().Token);
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
        public Task<string> GetOAuthUrl(string redirectUrl)
        {
            return Task.FromResult<string>(string.Format("{0}&client_id={1}&redirect_uri={2}",
                                    Settings.FacebookOauthEndpointWithPermissions,
                                    Settings.FacebookAppId,
                                    redirectUrl));
        }

        /// <summary>
        /// Get Access Token for current user
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        protected async Task<string> GetTokenFromStorage(string jobId)
        {
            string accessTokenkey = makeAccessTokenKey(jobId);
            tokenTable = azureTableProvider.GetAzureTableReference(Settings.TokenTableName);
            TokenTableEntity entity = await azureTableProvider.GetEntityAsync<TokenTableEntity>(tokenTable, accessTokenkey, ConnectorJobType.Facebook.ToString());
            return entity?.Token;
        }

        /// <summary>
        /// Delete Access Token for current user
        /// </summary>
        /// <param name="jobType"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public async Task DeleteToken(ConnectorJobType jobType, string jobId)
        {
            string accessTokenkey = makeAccessTokenKey(jobId);
            Task.Run(() => DeleteTokenHelper(accessTokenkey, jobType));
        }

        private async Task DeleteTokenHelper(string accessTokenKey, ConnectorJobType jobType)
        {
            tokenTable = azureTableProvider.GetAzureTableReference(Settings.TokenTableName);
            TokenTableEntity entity = await azureTableProvider.GetEntityAsync<TokenTableEntity>(tokenTable, accessTokenKey, jobType.ToString());

            if (entity == null)
            {
                return;
            }

            await azureTableProvider.DeleteEntityAsync(tokenTable, entity);
        }

        private string makeAccessTokenKey(string jobId)
        {
            return string.Format("{0}|{1}", jobId, ConnectorJobType.Facebook.ToString());
        }

        /// <summary>
        /// Subscribe page feed
        /// </summary>
        /// <param name="sourceInfoJson">source info has all the information of the page</param>
        /// <returns>Whether subscribed or not</returns>
        public async Task<bool> Subscribe(string sourceInfoJson)
        {
            SourceInfoFB sourceInfo = JsonConvert.DeserializeObject<SourceInfoFB>(sourceInfoJson);
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add(this.accessTokenParam, sourceInfo.AccessToken);

            SubscribeWebhookResponseFB response = await this.Client.PostRequestAsync<Dictionary<string, string>, SubscribeWebhookResponseFB>(sourceInfo.PageId + "/subscribed_apps", queryParams, new CancellationTokenSource().Token);

            if (response != null && response.Success == true)
            {
                return true;
            }

            Trace.TraceError("Subscription failed for page id {0}", sourceInfo.PageId);
            throw new Exception("Internal Server Error");
        }

        public async Task<bool> Unsubscribe(string sourceInfoJson)
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