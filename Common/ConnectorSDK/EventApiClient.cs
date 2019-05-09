// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class EventApiClient : IEventApiClient
    {
        private readonly string baseUrl;
        private readonly HttpClient httpClient;
        private readonly Auth auth;

        public EventApiClient(Auth auth, string baseUrl)
        {
            this.httpClient = new HttpClient();
            this.auth = auth;
            this.baseUrl = baseUrl;
        }

        public async Task OnDownloadCompleteAsync(string tenantId, string jobId, string taskId, Status status, List<ItemMetadata> itemMetadata)
        {
            Log($"Calling event API for task completion, jobId:{jobId}, taskId:{taskId}, status:{status}");
            DownloadComplete e = new DownloadComplete {
              jobId = jobId,
              taskId = taskId,
              status = status,
              itemMetadata = itemMetadata
            };
            Trace.TraceInformation(JsonConvert.SerializeObject(e));
            string token = await auth.GetTokenAsync(tenantId);
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Headers =
                {
                    {
                        "Authorization", "Bearer " + token
                    }
                },
                RequestUri = new Uri(baseUrl + "api/event/downloadcomplete/"),
                Content = new StringContent(JsonConvert.SerializeObject(e), Encoding.UTF8, "application/json")
            };
            await httpClient.SendAsync(request);
        }

        public async Task OnWebhookEvent(string tenantId, string jobId, string timeStamp, string itemId, string change)
        {
            Log($"Calling event API for change, jobId:{jobId}, itemId:{itemId}, change:{change}");
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            
            NativeConnetorEvent e = new NativeConnetorEvent()
            {
                Id = itemId,
                JobId = jobId,
                ChangeType = change,
                TimeStamp = unixEpoch.AddSeconds(Double.Parse(timeStamp))
            };
            NativeConnetorEventList list = new NativeConnetorEventList
            {
                entry = new List<NativeConnetorEvent> { e }
            };
            string token = await auth.GetTokenAsync(tenantId);
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Headers =
                {
                    {
                        "Authorization", "Bearer " + token
                    }
                },
                RequestUri = new Uri(baseUrl + "api/event/nativeconnector/"),
                Content = new StringContent(JsonConvert.SerializeObject(list), Encoding.UTF8, "application/json")
            };
            await httpClient.SendAsync(request);
        }

        private static void Log(string message, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            Trace.TraceInformation($"{file}#{member}({line}): {message}");
        }
    }
}
