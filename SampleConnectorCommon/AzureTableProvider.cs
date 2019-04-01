// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Microsoft.WindowsAzure.Storage.Table.Queryable;

    public class AzureTableProvider
    {
        private CloudTableClient tableClient;

        public AzureTableProvider(string connectionString)
            : this(CloudStorageAccount.Parse(connectionString))
        {

        }

        private AzureTableProvider(CloudStorageAccount storageAccount)
        {
            this.tableClient = storageAccount.CreateCloudTableClient();
        }

        // Create Table
        public async Task<CloudTable> EnsureTableExistAsync(string tableName)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        // Get table reference
        public CloudTable GetAzureTableReference(string tableName)
        {
            return tableClient.GetTableReference(tableName);
        }

        // Get specific entity
        public async Task<T> GetEntityAsync<T>(CloudTable table, string partitionKey, string rowKey)
            where T : TableEntity
        {
            return (await table.ExecuteAsync(TableOperation.Retrieve<T>(partitionKey, rowKey))).Result as T;
        }

        // Insert or replace entity
        public async Task InsertOrReplaceEntityAsync<T>(CloudTable table, T entity)
            where T : TableEntity
        {
            await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        // Insert or merge entity
        public async Task InsertOrMergeEntityAsync<T>(CloudTable table, T entity)
            where T : TableEntity
        {
            await table.ExecuteAsync(TableOperation.InsertOrMerge(entity));
        }

        // Insert entity
        public async Task InsertEntityAsync<T>(CloudTable table, T entity)
            where T : TableEntity
        {
            await table.ExecuteAsync(TableOperation.Insert(entity));
        }

        //Replace entity
        public async Task ReplaceEntityAsync<T>(CloudTable table, T entity)
            where T : TableEntity
        {
            await table.ExecuteAsync(TableOperation.Replace(entity));
        }

        //Merge entity
        public async Task MergeEntityAsync<T>(CloudTable table, T entity)
            where T : TableEntity
        {
            await table.ExecuteAsync(TableOperation.Merge(entity));
        }

        //Delete entity
        public async Task DeleteEntityAsync<T>(CloudTable table, T entity)
            where T : TableEntity
        {
            await table.ExecuteAsync(TableOperation.Delete(entity));
        }

        // Query entity based on filter condition
        public async Task<List<T>> QueryEntitiesAsync<T>(CloudTable table, Expression<Func<T, bool>> filter)
            where T : TableEntity, new()
        {
            IQueryable<T> query = table.CreateQuery<T>().Where(filter);

            TableQuery<T> tableQuery = query as TableQuery<T>;

            return await this.TableQueryExecuteAsync(table, tableQuery);
        }

        public async Task<List<T>> QueryEntitiesAsync<T>(CloudTable table, string filter)
            where T : TableEntity, new()
        {
            TableQuery<T> tableQuery = new TableQuery<T>().Where(filter);

            return await this.TableQueryExecuteAsync(table, tableQuery);
        }


        private async Task<List<T>> TableQueryExecuteAsync<T>(CloudTable table,
           TableQuery<T> query) where T : TableEntity , new()
        {
            var entities = new List<T>();
            TableContinuationToken token = null;
            do
            {
                // ExecuteSegmentedAsync returns a maximum of 1000 entities.
                TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync(query, token);

                token = segment.ContinuationToken;
                entities.Capacity += segment.Results.Count;
                entities.AddRange(segment.Results);

                if (token != null && query.TakeCount.HasValue)
                {
                    int newTakeCount = query.TakeCount.Value - segment.Results.Count;
                    query = newTakeCount > 0
                        ? query.Take<T>(newTakeCount).AsTableQuery()
                        : null;
                }
            }
            while (token != null && query != null);

            return entities;
        }
    }
}
