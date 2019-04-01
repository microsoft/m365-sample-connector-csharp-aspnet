// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Diagnostics;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    public class MonitoringActionFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Entry method for controller.
        /// </summary>
        /// <param name="actionContext">Context of the action.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);
            
            string actionName = actionContext?.ActionDescriptor?.ActionName ?? string.Empty;
            string controllerName = actionContext?.ControllerContext?.ControllerDescriptor?.ControllerName ?? string.Empty;

            Trace.TraceInformation($"Request for JobType {controllerName} and RequestType {actionName} received.");
            
            // Set time(when request arrives) in Request Header which is used to log response time metric in OnActionExecuted
            actionContext.Request.Headers.Date = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Method for post action processing.
        /// </summary>
        /// <param name="actionExecutedContext">Context of executed action.</param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            string actionName = actionExecutedContext.ActionContext?.ActionDescriptor?.ActionName ?? string.Empty;
            string controllerName = actionExecutedContext.ActionContext?.ControllerContext?.ControllerDescriptor?.ControllerName ?? string.Empty;

            if (actionExecutedContext.Response != null && actionExecutedContext.Response.IsSuccessStatusCode)
            {
                DateTime currentTime = DateTime.UtcNow;
                DateTimeOffset requestReceivedTime = actionExecutedContext.Request.Headers.Date.Value;
                long timeTaken = Convert.ToInt64((currentTime - requestReceivedTime.UtcDateTime).TotalMilliseconds);

                Trace.TraceInformation($"Request successfully processed for JobType {controllerName} and RequestType {actionName} in time {timeTaken} milliseconds.");
            }
            else
            {
                // All exceptions & errors occured while processing the request should be logged here.
                if (actionExecutedContext.Exception != null)
                    Trace.TraceError($"Request processing failed for JobType {controllerName} and RequestType {actionName}. ", actionExecutedContext.Exception);

                else if (actionExecutedContext.Response != null)
                    Trace.TraceError($"Request processing failed for JobType {controllerName} and RequestType {actionName} with error {actionExecutedContext.Response.ToString()}");

                else
                    Trace.TraceError($"Unknown Error occurred for JobType {controllerName} and RequestType {actionName}.");
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}