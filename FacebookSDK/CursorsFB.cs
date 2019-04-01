// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// cursors in FBPaging. Actual pointers pointing to next and previous data in Paginated Results  
    /// </summary>
    public class CursorsFB
    {
        /// <summary>
        /// after pointer in cursor object
        /// </summary>
        [JsonProperty("after")]
        public string After { get; set; }

        /// <summary>
        /// before pointer in cursor object
        /// </summary>
        [JsonProperty("before")]
        public string Before { get; set; }
    }
}
