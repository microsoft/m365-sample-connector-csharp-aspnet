// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods for use by Connector Service to upload data to O365 Import.
    /// </summary>
    public interface IUploader
    {
        Task<string> UploadItem(string jobId, string taskId, Item item);
    }
}