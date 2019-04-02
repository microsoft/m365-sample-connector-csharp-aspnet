// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using System;
    using System.Threading.Tasks;

    public class AzureStorageQueueProvider
    {
        private CloudQueue queue;

        public AzureStorageQueueProvider(string connectionString, string queueName)
            : this(CloudStorageAccount.Parse(connectionString), queueName)
        {

        }
        
        // private constructor using storageaccount
        private AzureStorageQueueProvider(CloudStorageAccount storageAccount, string queueName)
        {
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            this.queue = CreateIfNotExist(queueClient, queueName);
        }

        private CloudQueue CreateIfNotExist(CloudQueueClient queueClient, string queueName)
        {
            CloudQueue queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExists();
            return queue;
        }

        /// <summary>
        /// Insert Message into the queue
        /// </summary>
        /// <param name="message">Message as string</param>
        public Task InsertMessageAsync(string message)
        {
            CloudQueueMessage cloudMessage = new CloudQueueMessage(message);
            return this.queue.AddMessageAsync(cloudMessage);
        }

        /// <summary>
        /// Returns the next item on the queue. Note that this will not delete the message from the
        /// queue.
        /// </summary>
        /// <param name="visibilityTimeout">The visibility time out after which the message will be
        /// visible again.</param>
        /// <returns>The queue message.</returns>
        public CloudQueueMessage GetMessage(TimeSpan visibilityTimeout)
        {
            return this.queue.GetMessage(visibilityTimeout);
        }

        public void DeleteMessage(CloudQueueMessage cloudMessage)
        {
            this.queue.DeleteMessage(cloudMessage);
        }
    }
}