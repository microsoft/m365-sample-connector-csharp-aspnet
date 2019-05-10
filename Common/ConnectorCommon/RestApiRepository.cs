// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Threading;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Net.Http.Headers;

    /// <summary>
    /// Helper class that provides properties and methods to help call REST Api.
    /// </summary>
    public class RestApiRepository : IRestApiRepository
    {
        /// <summary>
        /// Http client for sending Web Requests
        /// </summary>
        public HttpClient HttpClient { get; private set; }

        public string baseAddress { get; private set; }
        
        public RestApiRepository(string baseAddress)
        {
            this.baseAddress = baseAddress;
            HttpClient = NewHttpJsonClient(baseAddress, DefaultHttpMessageHandler());
        }

        private static HttpClientHandler DefaultHttpMessageHandler()
        {
            return new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
        }

        public RestApiRepository()
        {
        }

        private static HttpClient NewHttpJsonClient(string baseAddress, HttpMessageHandler handler)
        {
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(baseAddress);
            return client;
        }

        public Uri CreateRequestUri(string resource, Dictionary<string, string> queryParams)
        {
            UriBuilder requestUri = new UriBuilder($"{this.baseAddress}/{resource}");
            NameValueCollection query = HttpUtility.ParseQueryString(requestUri.Query);

            if (queryParams != null)
            {
                foreach (KeyValuePair<string, string> entry in queryParams)
                    query.Add(entry.Key, entry.Value);
            }

            requestUri.Query = query == null ? string.Empty : query.ToString();
            return requestUri.Uri;
        }

        public async Task<R> GetRequestAsync<R>(string requestUri, Dictionary<string, string> headers, Dictionary<string, string> request, CancellationToken cancellationToken)
        {
            var response = await GetRequestAsync(requestUri, headers, request, cancellationToken);
            return await GetResultFromResponse<R>(response);
        }

        private async Task<HttpResponseMessage> GetRequestAsync(string resource, Dictionary<string, string> headers, Dictionary<string, string> queryParams, CancellationToken cancellationToken)
        {
            HttpClient httpClient = this.HttpClient;
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            Uri requestUri = CreateRequestUri(resource, queryParams);
            var response = await httpClient.GetAsync(requestUri.ToString(), cancellationToken);
            return response;
        }
        
        public async Task<R> DeleteRequestAsync<R>(string requestUri, Dictionary<string, string> request, CancellationToken cancellationToken)
        {
            var response = await DeleteRequestAsync(requestUri, request, cancellationToken);
            return await GetResultFromResponse<R>(response);
        }

        private async Task<HttpResponseMessage> DeleteRequestAsync(string resource, Dictionary<string, string> queryParams, CancellationToken cancellationToken)
        {
            Uri requestUri = CreateRequestUri(resource, queryParams);
            var response = await this.HttpClient.DeleteAsync(requestUri.ToString(), cancellationToken);
            return response;
        }

        public async Task<R> PostRequestAsync<T, R>(string requestUri, Dictionary<string, string> headers, T request, CancellationToken cancellationToken)
        {
            var response = await PostRequestAsync<T>(requestUri, headers, request, cancellationToken);
            return await GetResultFromResponse<R>(response);
        }

        private async Task<HttpResponseMessage> PostRequestAsync<T>(string requestUri, Dictionary<string, string> headers, T request, CancellationToken cancellationToken)
        {
            var httpClient = this.HttpClient;
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(requestUri, request, cancellationToken);
            return response;
        }

        private static async Task<R> GetResultFromResponse<R>(HttpResponseMessage response)
        {
            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var result = JsonConvert.DeserializeObject<R>(jsonResponse, GetJsonSerializerSettings());
                        return result;
                    }
                    catch(Exception)
                    {
                        var obj = (Object)jsonResponse;
                        return (R)obj;
                    }
                }
                else
                {
                    throw TranslateErrorResponse(response);
                }
            }
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings();
            var converters = settings.Converters;
            settings.NullValueHandling = NullValueHandling.Ignore;
            return settings;
        }

        private static Exception TranslateErrorResponse(HttpResponseMessage response)
        {
            Debug.Assert(!response.IsSuccessStatusCode);
            throw new RestClientException(response.StatusCode);
        }
    }

    public class RestClientException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; private set; }
        public RestClientException(HttpStatusCode httpStatusCode)
        {
            this.HttpStatusCode = httpStatusCode;
        }

        protected RestClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}