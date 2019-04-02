// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Diagnostics;

    /// <summary>
    /// This class represents http utilities
    /// </summary>
    public class Downloader : IDownloader
    {
        /// <summary>
        /// Http Client for sending Web Requests
        /// </summary>
        private HttpClient httpClient;

        /// <summary>
        /// Constructor
        /// </summary>
        public Downloader()
        {
            this.httpClient = new HttpClient();
        }

        /// <summary>
        /// Sends a web request to provided url and captures the response.
        /// </summary>
        /// <typeparam name="Response">the response</typeparam>
        /// <typeparam name="Error">the error</typeparam>
        /// <param name="requestUrl">the requestUrl</param>
        /// <param name="header">header to add</param>
        /// <returns>Returns the desired response.</returns>
        public async Task<Response> GetWebContent<Response, Error>(string requestUrl, AuthenticationHeaderValue header)
        {
            httpClient.DefaultRequestHeaders.Authorization = header;

            using (HttpResponseMessage response = await this.httpClient.GetAsync(requestUrl))
            {
                return await HandleResponse<Response, Error>(response);
            }
        }
        
        /// <summary>
        /// Downloads a file from a specified Internet address.
        /// </summary>
        /// <param name="resourceUrl">Internet address of the file to download.</param>
        /// <returns>the downloaded content.</returns>
        public async Task<string> DownloadFileAsBase64EncodedString(string resourceUrl)
        {
            byte[] content = await this.httpClient.GetByteArrayAsync(resourceUrl);
            return Convert.ToBase64String(content);
        }

        /// <summary>
        /// Handles the response
        /// </summary>
        /// <typeparam name="Response">the response</typeparam>
        /// <typeparam name="Error">the error</typeparam>
        /// <param name="response">returned response</param>
        /// <returns>the response</returns>
        private static async Task<Response> HandleResponse<Response, Error>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return await GetResultFromResponse<Response>(response);
            }
            else
            {
                var exception = await GetExceptionFromResponse<Error>(response);
                Trace.TraceError($"Exception while handling GetWebContent Response, {exception.Message.ToString()}");
                throw exception;
            }
        }

        /// <summary>
        /// Handles response
        /// </summary>
        /// <typeparam name="Error"></typeparam>
        /// <param name="response">returned response</param>
        /// <returns>returns exception</returns>
        private static async Task<Exception> GetExceptionFromResponse<Error>(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    Error e = await GetResultFromResponse<Error>(response);
                    return new ClientException<Error>(e);
                default:
                    return new HttpRequestException(response.ToString() + await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Deserializes the response
        /// </summary>
        /// <typeparam name="Response"></typeparam>
        /// <param name="response"></param>
        /// <returns>the deserialized response</returns>
        private static async Task<Response> GetResultFromResponse<Response>(HttpResponseMessage response)
        {
            using (response)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Response>(jsonResponse, GetJsonSerializerSettings());
                return result;
            }
        }

        /// <summary>
        /// Json serializer settings
        /// </summary>
        /// <returns>the settings</returns>
        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings();
            var converters = settings.Converters;
            settings.NullValueHandling = NullValueHandling.Ignore;
            return settings;
        }
    }
}