// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Web;
    using System.Web.Routing;
    using System.Web.WebPages;

    public class DefaultRouteHandler : IRouteHandler
    {
        /// <summary>
        /// Gets the handler for given request.
        /// </summary>
        /// <param name="requestContext">The request.</param>
        /// <returns>The handler for the input request.</returns>
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var filePath = requestContext.HttpContext.Request.AppRelativeCurrentExecutionFilePath;

            if (filePath == "~/")
            {
                filePath = "~/index.cshtml";
            }
            else
            {
                if (!filePath.StartsWith("~/views/", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = filePath.Insert(2, "Views/");
                }

                if (!filePath.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = filePath += ".cshtml";
                }
            }

            IHttpHandler handler = WebPageHttpHandler.CreateFromVirtualPath(filePath); // returns NULL if .cshtml file wasn't found
            if (handler == null)
            {
                requestContext.RouteData.DataTokens.Add("templateUrl", "/views/404");
                handler = WebPageHttpHandler.CreateFromVirtualPath("~/views/Shared/Error.cshtml");
            }
            else
            {
                requestContext.RouteData.DataTokens.Add("templateUrl", filePath.Substring(1, filePath.Length - 8));
            }

            return handler;
        }
    }
}