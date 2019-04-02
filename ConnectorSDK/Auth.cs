// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class Auth
    {
        private static readonly string aadInstance = "https://login.windows.net/";
        private static readonly string resourceId = "https://microsoft.onmicrosoft.com/4e476d41-2395-42be-89ff-34cb9186a1ac";

        private readonly string appId;
        private readonly string appSecret;

        public Auth(string appId, string appSecret)
        {
            this.appId = appId;
            this.appSecret = appSecret;
        }

        public  async Task<string> GetTokenAsync(string tenantId)
        {
            var cred = new ClientCredential(appId, appSecret);
            AuthenticationContext authenticationContext = new AuthenticationContext($"{aadInstance}{tenantId}");
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync(resourceId, cred);
            return authenticationResult.AccessToken;
        }
    }
}
