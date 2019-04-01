// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.Test
{
    /// <summary>
    /// Cases in Unit Tests
    /// </summary>
    public enum TestCases
    {
        /// <summary>
        /// Successfully Fetched 
        /// </summary>
        DummyPosts = 1,

        /// <summary>
        /// Error while fetching 
        /// </summary>
        DummyError = 2,

        /// <summary>
        /// Empty List
        /// </summary>
        NoPostsFetched = 3,
    }
}
