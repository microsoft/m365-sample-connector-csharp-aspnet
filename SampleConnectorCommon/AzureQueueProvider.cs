// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Queue.Protocol;
    using System;
    using System.Threading.Tasks;

    public class AzureStorageQueueProvider
    {
        private CloudQueue queue;

        public AzureStorageQueueProvider(string connectionString, string queueName)
            : this(CloudStorageAccount.Parse(connectionString), queueName)
        {

        }

        public AzureStorageQueueProvider(string queueSasUri)
        {
            queue = new CloudQueue(new Uri(queueSasUri));
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

        public void InsertMessage(string message)
        {
            CloudQueueMessage cloudMessage = new CloudQueueMessage(message);
            this.queue.AddMessage(cloudMessage);
        }


        /// <summary>
        /// Returns the next item on the queue. Note that this will not delete the message from the
        /// queue. The message will become invisible for 30 seconds (default time period).
        /// </summary>
        /// <returns>The queue message. Null if no message is available in the queue.</returns>
        public CloudQueueMessage GetMessage()
        {
            return this.queue.GetMessage();
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

        public void SetQueuePermission(string policyIdentifier, DateTime accessExpiryTime, SharedAccessQueuePermissions accessPermission)
        {
            var perm = new QueuePermissions();
            var policy = new SharedAccessQueuePolicy { SharedAccessExpiryTime = accessExpiryTime, Permissions = accessPermission };

            perm.SharedAccessPolicies.Add(policyIdentifier, policy);

            this.queue.SetPermissions(perm);
        }

        public string GetQueueSharedAccessSignature(SharedAccessQueuePolicy accessPolicy, string policyIdentifier)
        {
            return this.queue.GetSharedAccessSignature(accessPolicy, policyIdentifier);
        }

    }
}