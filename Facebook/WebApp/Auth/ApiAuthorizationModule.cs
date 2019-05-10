// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    public class ApiAuthorizationModule : AuthorizeAttribute, IAuthorizationFilter
    {
        public async Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            bool isAuthenticated = await AuthenticateRequest.AuthenticateRequestAsync(actionContext);
            if (isAuthenticated)
            {
                return await continuation();
            }

            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

    }
}