// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.Test
{
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Net.Http.Headers;
    using System;

    public class FakeDownloader : IDownloader
    {
        /// <summary>
        /// Different Test Cases
        /// </summary>
        TestCases cases;

        /// <summary>
        /// constructor
        /// </summary>
        public FakeDownloader(TestCases cases)
        {
            this.cases = cases;
        }

        /// <summary>
        /// Fake Content Downloader
        /// </summary>
        /// <typeparam name="Response">the response</typeparam>
        /// <typeparam name="Error">the error</typeparam>
        /// <param name="requestUrl">the requestUrl</param>
        /// <param name="header">Header value to be added</param>
        /// <returns>Returns the desired response.</returns>
        public async Task<Response> GetWebContent<Response, Error>(string url, AuthenticationHeaderValue header)
        {
            switch (cases)
            {
                case TestCases.DummyPosts:
                    if (!url.Equals("NextPageURL"))
                    {
                        return await Task.FromResult(JsonConvert.DeserializeObject<Response>(File.ReadAllText(@"FakeData\FakeData.json")));
                    }
                    else
                    {
                        return await Task.FromResult(JsonConvert.DeserializeObject<Response>("{\"data\": []}"));
                    }
                case TestCases.DummyError:
                    throw new ClientException<Error>(JsonConvert.DeserializeObject<Error>(File.ReadAllText(@"FakeData\FakeError.json")));
                case TestCases.NoPostsFetched:
                    return await Task.FromResult(JsonConvert.DeserializeObject<Response>("{\"data\": []}"));
                default:
                    return await Task.FromResult(JsonConvert.DeserializeObject<Response>("{\"data\": []}"));
            }
        }
        
        /// <summary>
        /// Fake Attachment Downloader
        /// </summary>
        /// <param name="resourceUrl"></param>
        /// <returns></returns>
        public async Task<string> DownloadFileAsBase64EncodedString(string resourceUrl)
        {
            string content;
            using (Stream fs = new FileStream(@"FakeData\FakeImage.jpg", FileMode.Open))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await fs.CopyToAsync(ms);
                    content = Convert.ToBase64String(ms.ToArray());
                }
            }
            return content;
        }
    }
}
