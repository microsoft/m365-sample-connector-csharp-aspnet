// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;

    public class AuthenticateRequest
    {
        public static async Task<bool> AuthenticateRequestAsync(HttpActionContext actionContext)
        {
            bool isAuthorized = false;

            HttpRequestHeaders headers = actionContext.Request.Headers;
            string authScheme = headers.Authorization.Scheme;

            if (authScheme.Equals("Bearer"))
            {
                if (actionContext.RequestContext != null)
                {
                    ClaimsPrincipal claimsPrincipal = actionContext.RequestContext.Principal as ClaimsPrincipal;

                    if (claimsPrincipal != null && claimsPrincipal.Claims != null)
                    {
                        isAuthorized = CheckIfCallerClaimIsAuthorized(claimsPrincipal.Claims);
                    }
                }
                return isAuthorized;
            }
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Authorization
            else if (authScheme.Equals("Basic"))
            {
                try
                {
                    string decodedCredentials = new ASCIIEncoding().GetString(Convert.FromBase64String(headers.Authorization.Parameter));
                    string[] userPass = decodedCredentials.Split(new char[] { ':' });
                    if (userPass[0].Equals(Settings.TenantId) && userPass[1].Equals(Settings.APISecretKey))
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Checks if the calling client is whitelisted or not
        /// </summary>
        /// <param name="currentClaims">Current claims</param>
        /// <returns>True if the claims are valid; otherwise false.</returns>
        private static bool CheckIfCallerClaimIsAuthorized(IEnumerable<Claim> currentClaims)
        {
            bool isAuthorized = false;

            // application id of microsoft o365 import connector platform
            string importClientId = "570d0bec-d001-4c4e-985e-3ab17fdc3073";
            Claim appIdClaim = currentClaims.FirstOrDefault(c => c != null && c.Type != null && c.Type.ToLower().Equals("appid"));

            if (appIdClaim != null && !string.IsNullOrEmpty(appIdClaim.Value))
            {
                isAuthorized = importClientId.Equals(appIdClaim.Value);
            }
            string tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            return isAuthorized && (tenantId != null);
        }
    }
}