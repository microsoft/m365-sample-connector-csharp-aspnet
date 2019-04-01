// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Request payload for the downloadcomplete endpoint of Event Api
    /// </summary>
    public class DownloadComplete
    {
        public string jobId;
        public string taskId;
        public Status status;

        // Captures the metadata of all the items that have been downloaded from data source & uploaded to O365 blob container
        public List<ItemMetadata> itemMetadata;
    }

    public class ItemMetadata
    {
        public string id;
        public DateTime modificationTimestampUtc;
        public string fileName;

        public ItemMetadata(string id, DateTime modificationTimeStampUtc, string fileName)
        {
            this.id = id;
            this.modificationTimestampUtc = modificationTimeStampUtc;
            this.fileName = fileName;
        }
    }

    public enum Status
    {
        Success = 0,

        TemporaryFailure = 1,

        PermanentFailure = 2,
    }

    /// <summary>
    /// Request payload for the nativeconnector endpoint of Event Api
    /// </summary>
    public class NativeConnetorEventList
    {
        public List<NativeConnetorEvent> entry { get; set; }
    }

    public class NativeConnetorEvent
    {
        public string Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public string ChangeType { get; set; }

        public string JobId { get; set; }
    }

}
