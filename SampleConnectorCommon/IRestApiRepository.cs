// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRestApiRepository
    {
        Task<R> GetRequestAsync<R>(string requestUri, Dictionary<string, string> request,CancellationToken cancellationToken);
        Task<R> PostRequestAsync<T, R>(string requestUri, T request, CancellationToken cancellationToken);
        Task<R> DeleteRequestAsync<R>(string requestUri, Dictionary<string, string> request, CancellationToken cancellationToken);
    }
}