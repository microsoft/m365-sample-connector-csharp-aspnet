// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Filters;

    /// <summary>
    /// Global Exception filter
    /// </summary>
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        /// <summary>
        /// All exception inside fusion api sevrice will be handled here.
        /// Custom exception can be handled.
        /// </summary>
        /// <param name="filterContext">Represents the action of the HTTP executed context</param>
        public override void OnException(HttpActionExecutedContext filterContext)
        {
            // Unhandled errors
            filterContext.Response = createErrorResponse(HttpStatusCode.InternalServerError, "Internal server error", filterContext);

            Trace.TraceError($"Exception thrown {filterContext.Exception.GetBaseException().Message}");
            Trace.TraceError($"Exception Stack trace {filterContext.Exception.StackTrace}");
            base.OnException(filterContext);
        }

        /// <summary>
        /// Create custom response
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="message">Error message</param>
        /// <param name="context">Request context</param>
        /// <returns></returns>
        private HttpResponseMessage createErrorResponse(HttpStatusCode code, string message, HttpActionExecutedContext context )
        {
            return context.Request.CreateResponse(code, new { Message = message });
        }
    }
}