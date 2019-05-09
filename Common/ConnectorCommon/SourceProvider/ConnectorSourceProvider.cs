// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sample.Connector
{

    public  abstract class ConnectorSourceProvider : IConnectorSourceProvider
    {
        protected AzureTableProvider azureTableProvider;
        private CloudTable tokenTable;

        public ConnectorSourceProvider(AzureTableProvider azureTableProvider, CloudTable tokenTable)
        {
            this.azureTableProvider = azureTableProvider;
            this.tokenTable = tokenTable;
        }

        public async Task DeleteToken(string connectorJobType, string jobId)
        {
            string accessTokenKey = makeAccessTokenKey(jobId, connectorJobType);
            Task.Run(() => DeleteTokenHelper(accessTokenKey, connectorJobType));
        }

        private async Task DeleteTokenHelper(string accessTokenKey, string connectorJobType)
        {
            TokenTableEntity entity = await azureTableProvider.GetEntityAsync<TokenTableEntity>(tokenTable, accessTokenKey, connectorJobType);

            if (entity == null)
            {
                return;
            }

            await azureTableProvider.DeleteEntityAsync(tokenTable, entity);
        }

        public abstract Task<string> GetAuthTokenForResource(string resourceId, string jobId);

        public abstract Task<IEnumerable<ConnectorEntity>> GetEntities(string jobId);

        public abstract Task<string> GetOAuthUrl(string redirectUrl);

        public abstract Task StoreOAuthToken(string accessCode, string redirectUrl, string jobId);

        public abstract Task<bool> Subscribe(string sourceInfo);

        public abstract Task<bool> Unsubscribe(string sourceInfo);
        
        protected async Task AddTokenIntoStorage(string jobId, string accessToken, string connectorJobType)
        {
            string accessTokenKey = makeAccessTokenKey(jobId, connectorJobType);
            TokenTableEntity entity = new TokenTableEntity(accessTokenKey, connectorJobType);
            entity.Token = accessToken;
            await azureTableProvider.InsertOrReplaceEntityAsync(tokenTable, entity);
        }

        protected async Task<string> GetTokenFromStorage(string jobId, string connectorJobType)
        {
            string accessTokenkey = makeAccessTokenKey(jobId, connectorJobType);
            TokenTableEntity entity = await azureTableProvider.GetEntityAsync<TokenTableEntity>(tokenTable, accessTokenkey, connectorJobType);
            return entity?.Token;
        }

        private string makeAccessTokenKey(string jobId, string connectorJobType)
        {
            return $@"{jobId}|{connectorJobType}";
        }
    }
}
