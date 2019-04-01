// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Net;
    using System.Text;
    using System.Web.Http;
    using System.IO;
    using System.Web;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using FacebookSDK;

    public class FbPageWebhookController : ApiController
    {
        private string verifyToken;

        private string appSecret;

        private readonly ConnectorSourceFactory connectorSourceFactory;

        private IEventApiClient eventApiClient;

        private Dictionary<string,PageJobEntity> PageJobMapping = new Dictionary<string, PageJobEntity>();

        private readonly AzureTableProvider azureTableProvider;

        private CloudTable PageJobMappingTable;

        /// <summary>
        /// Facebook webhook controller
        /// </summary>
        public FbPageWebhookController()
        {
            verifyToken = Settings.FacebookVerifyToken;
            appSecret = Settings.FacebookAppSecret;

            // Can be done using Dependency Injection
            eventApiClient = new EventApiClient(new Auth(Settings.AAdAppId, Settings.AAdAppSecret), Settings.EventAPIBaseUrl);
            connectorSourceFactory = new ConnectorSourceFactory();
            azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
        }

        /// <summary>
        /// Get request while configuring the webhook product
        /// </summary>
        /// <returns>Http Response Message</returns>
        [HttpGet]
        public HttpResponseMessage Get()
        {
            Trace.TraceInformation("FbPageWebhookController verify");

            if (HttpContext.Current.Request.QueryString["hub.verify_token"].Equals(this.verifyToken))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(HttpContext.Current.Request.QueryString["hub.challenge"])
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                Trace.TraceInformation("FbPageWebhookController authorized");
                return response;
            }
            else
            {
                Trace.TraceWarning("FbPageWebhookController Unauthorized");
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }

        /// <summary>
        /// Actual payload post request
        /// </summary>
        /// <param name="data">Payload coming from facebook</param>
        /// <returns>Http Response Message</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] WebhookFeedFB data)
        {
            StreamReader reader = new StreamReader(HttpContext.Current.Request.InputStream);
            string json = reader.ReadToEnd();
            bool isValid = ValidateSignatureWithPayload(json, HttpContext.Current.Request.Headers["X-Hub-Signature"], this.appSecret);
            if (isValid)
            {
                await SendDataAsync(data);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
        
        /// <summary>
        /// convert facebook data to table object
        /// </summary>
        /// <param name="data">the data comng from server</param>
        /// <returns>Table object</returns>
        private async Task SendDataAsync(WebhookFeedFB data)
        {
            string jobId, tenantId;
            foreach (Entry entry in data.Entry)
            {
                if (PageJobMapping.ContainsKey(entry.Id))
                {
                    jobId = PageJobMapping[entry.Id].RowKey;
                    tenantId = PageJobMapping[entry.Id].TenantId;
                }
                else
                {
                    Trace.TraceInformation($"Page Id: {entry.Id}");
                    Expression<Func<PageJobEntity, bool>> filter = (entity => entity.PartitionKey == entry.Id);
                    PageJobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);
                    List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(PageJobMappingTable, filter);
                    Trace.TraceInformation($"Fetched entries from Table: {pageJobEntityList.Count}");
                    PageJobEntity pageJobEntity = pageJobEntityList?[0];
                    PageJobMapping.Add(pageJobEntity?.PartitionKey, pageJobEntity);
                    jobId = pageJobEntity?.RowKey;
                    tenantId = pageJobEntity?.TenantId;
                }

                foreach (Change change in entry.Changes)
                {
                    if (change.Value != null  && change.Value.PostId != null)
                    {
                        await eventApiClient.OnWebhookEvent(tenantId, jobId, $"{change.Value?.CreatedTime}", $"{change.Value?.PostId}", "update");
                    }
                    else
                    {
                        Trace.TraceWarning("No post id");
                    }
                }
            }
        }

        /// <summary>
        /// The HTTP request will contain an X-Hub-Signature header which contains the SHA1 signature of the request payload,
        /// using the app secret as the key, and prefixed with sha1=.
        /// Your callback endpoint can verify this signature to validate the integrity and origin of the payload
        /// </summary>
        /// <param name="payload">payload</param>
        /// <param name="signature">signature of the json</param>
        /// <param name="appSecret">app secret</param>
        /// <returns>whether equal or not</returns>
        private bool ValidateSignatureWithPayload(string payload, string signature, string appSecret)
        {
            byte[] hmac = SignWithHmac(UTF8Encoding.UTF8.GetBytes(payload), UTF8Encoding.UTF8.GetBytes(appSecret));
            var hmacHex = ConvertToHexadecimal(hmac);

            return signature.Split('=')[1] == hmacHex;
        }

        /// <summary>
        /// Hmac signin with
        /// </summary>
        /// <param name="dataToSign">payoad to compute</param>
        /// <param name="keyBody">key</param>
        /// <returns>bytes of computehash</returns>
        private static byte[] SignWithHmac(byte[] dataToSign, byte[] keyBody)
        {
            using (var hmacAlgorithm = new System.Security.Cryptography.HMACSHA1(keyBody))
            {
                return hmacAlgorithm.ComputeHash(dataToSign);
            }
        }

        /// <summary>
        /// Convert bytes to hex
        /// </summary>
        /// <param name="bytes">bytes</param>
        /// <returns>string value of hex</returns>
        private static string ConvertToHexadecimal(byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}