// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Azure blob based Item uploader. The items will be uploaded following the naming convention specified by the protocol.
    /// </summary>
    public class BlobUploader : IUploader
    {
        private readonly CloudBlobContainer container;

        /// <summary>
        /// Create an instance of blob based uploader.
        /// </summary>
        /// <param name="containerSasUri">Sas Uri of the blob container to upload the items. Note that write permission should be given by this Sas</param>
        public BlobUploader(string containerSasUri)
        {
            container = new CloudBlobContainer(new Uri(containerSasUri));
        }

        public async Task<string> UploadItem(string jobId, string taskId, Item item)
        {
            CloudBlobDirectory jobDirectory = container.GetDirectoryReference(jobId);
            CloudBlobDirectory taskDirectory = jobDirectory.GetDirectoryReference(taskId);
            string fileName = "" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ".json";
            CloudBlockBlob blockBlob = taskDirectory.GetBlockBlobReference(fileName);
            await blockBlob.UploadTextAsync(JsonConvert.SerializeObject(item, Formatting.Indented, new VersionConverter()));
            return fileName;
        }
    }
}
