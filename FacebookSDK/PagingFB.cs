// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Paginated Data
    /// </summary>
    public class PagingFB
    {
        /// <summary>
        /// cursors in FBPaging. Actual pointers pointing to next and previous data in Paginated Results  
        /// </summary>
        [JsonProperty("cursors")]
        public CursorsFB Cursors { get; set; }

        /// <summary>
        /// url to next data in Paginated Results
        /// </summary>
        [JsonProperty("next")]
        public string Next { get; set; }

        /// <summary>
        /// url to previous data in Paginated Results
        /// </summary>
        [JsonProperty("previous")]
        public string Previous { get; set; }
    }
}
