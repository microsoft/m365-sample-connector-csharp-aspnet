// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    /// <summary>
    /// FB Source info Model
    /// </summary>
    public class SourceInfoFB
    {
        /// <summary>
        /// Gets or sets list of Pages
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Page id 
        /// </summary>
        public string PageId { get; set; }

        /// <summary>
        /// Page name
        /// </summary>
        public string PageName { get; set; }
    }
}