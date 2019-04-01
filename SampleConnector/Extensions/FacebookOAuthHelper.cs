// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;

namespace Sample.Connector
{
    public class FacebookOAuthHelper
    {
        private static readonly string JobType = "Facebook";

        /// <summary>
        /// Cookie key for nonce token
        /// </summary>
        private const string FacebookOAuthValidationToken = "FB-Oauth-Validation-Token";

        /// <summary>
        /// Source Provider Factory
        /// </summary>
        private static readonly ConnectorSourceFactory connectorSourceFactory =  new ConnectorSourceFactory();

        /// <summary>
        /// Authenticates Facebook login, gets access codes for pages and redirects user to Import page
        /// </summary>
        /// <param name="code">temporary facebook access code</param>
        /// <param name="url">Current request Url</param>
        /// <returns>Redirect to Import page with authentication acknowledgment</returns>
        public static Task<bool> StoreToken(IOwinContext context, string code, Uri url)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Task.FromResult<bool>(false);
            }

            RequestCookieCollection cookies = context.Request.Cookies;
            string jobId = cookies["jobId"];

            string redirectUrl = url.GetLeftPart(UriPartial.Path);

            return Task.Run(() => StoreTokenHelper(ConnectorJobType.Facebook, code, redirectUrl, jobId));
        }

        public static async Task<bool> StoreTokenHelper(ConnectorJobType jobType, string code, string redirectUrl, string jobId)
        {
            IConnectorSourceProvider sourceProvider = connectorSourceFactory.GetConnectorSourceInstance(jobType);
            await sourceProvider.StoreOAuthToken(code, redirectUrl, jobId);

            return true;
        }

        /// <summary>
        /// Redirect user to import page with current job opened
        /// </summary>
        /// <param name="context">Current OWin Context</param>
        /// <param name="state">State param</param>
        /// <returns>Redirects to specified page</returns>
        public static bool IsAuthenticRequest(IOwinContext context, string state)
        {
            RequestCookieCollection cookies = context.Request.Cookies;
            string stateCookieValue = HttpUtility.UrlDecode(cookies[FacebookOAuthValidationToken]);
            if (!string.IsNullOrEmpty(stateCookieValue) && state == stateCookieValue)
            {
                context.Response.Cookies.Delete(FacebookOAuthValidationToken);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Redirect user to import page with current job opened
        /// </summary>
        /// <param name="context">Current OWin Context</param>
        /// <param name="encodedUrl">Encoded Url to be redirected </param>
        /// <returns>Redirects to specified page</returns>
        public static string GetOAuthUrl(IOwinContext context, string encodedUrl)
        {
            string decodedUrl = HttpUtility.UrlDecode(encodedUrl);
            RequestCookieCollection cookies = context.Request.Cookies;
            string jobId = cookies["jobId"];
            string stateToken = CreateAndSetStateCookie(context);
            return string.Format("{0}&state={1}&jobId={2}", decodedUrl, stateToken, jobId);
        }

        /// <summary>
        /// Create a nonce token and store in cookie
        /// </summary>
        /// <param name="context">Current OWin Context</param>
        /// <returns>Generate nonce token</returns>
        private static string CreateAndSetStateCookie(IOwinContext context)
        {
            Guid g = Guid.NewGuid();
            string guidString = Convert.ToBase64String(g.ToByteArray());
            guidString = HttpUtility.UrlEncode(guidString);
            context.Response.Cookies.Append(
                FacebookOAuthValidationToken,
                guidString,
                new CookieOptions() { Path = context.Request.PathBase.Value, Secure = context.Request.IsSecure, HttpOnly = true });
            return guidString;
        }
    }
}