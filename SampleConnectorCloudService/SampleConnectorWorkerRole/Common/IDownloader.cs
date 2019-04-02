// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Threading.Tasks;
    using System.Net.Http.Headers;

    /// <summary>
    /// Downloader Interface
    /// </summary>
    public interface IDownloader
    {
        /// <summary>
        /// Sends a web request to provided url with provided Header and captures the response.
        /// </summary>
        /// <typeparam name="Response">Parameter 1</typeparam>
        /// <typeparam name="Error">Parameter 2</typeparam>
        /// <param name="url">the requestUrl</param>
        /// <param name="header">Header to add.</param>
        /// <returns>Returns the desired response</returns>
        Task<Response> GetWebContent<Response, Error>(string url, AuthenticationHeaderValue header);
        
        /// <summary>
        /// Downloads a file from a specified Internet address.
        /// </summary>
        /// <param name="resourceUrl">Internet address of the file to download.</param>
        /// <returns>the downloaded content.</returns>
        Task<string> DownloadFileAsBase64EncodedString(string resourceUrl);
    }
}
