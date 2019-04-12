namespace Sample.Connector
{
    using System;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Net.Http;
    using Microsoft.WindowsAzure.Storage.Table;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class Program
    {
        private static HttpClient httpClient = new HttpClient();
        private static AzureStorageQueueProvider queueProvider;
        private static AzureTableProvider azureTableProvider;
        private static CloudTable pageJobMappingTable;
        private static TimeSpan sleepTime = TimeSpan.FromSeconds(Convert.ToInt32(Settings.SleepTimeInSec));
        private static TimeSpan queueVisibilityTimeOutInSec = TimeSpan.FromSeconds(Convert.ToInt32(Settings.QueueVisibilityTimeOutInSec));

        public static void Main()
        {
            ConsoleTraceListener consoleTracer = new ConsoleTraceListener();
            Trace.Listeners.Add(consoleTracer);
            Trace.TraceInformation("Starting Azure Web Job");
            ProcessQueueMessage().Wait();
        }

        public async static Task ProcessQueueMessage()
        {
            queueProvider = new AzureStorageQueueProvider(Settings.StorageAccountConnectionString, Settings.QueueName);
            azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            pageJobMappingTable = await azureTableProvider.EnsureTableExistAsync(Settings.PageJobMappingTableName);
            while (true)
            {
                try
                {
                    CloudQueueMessage queueMessage = queueProvider.GetMessage(queueVisibilityTimeOutInSec);

                    if (queueMessage != null)
                    {
                        await ProcessMessage(queueMessage, httpClient, pageJobMappingTable);

                        // Delete the message
                        queueProvider.DeleteMessage(queueMessage);
                    }
                    else
                    {
                        await Task.Delay(sleepTime);
                    }
                }
                catch (Exception ex)
                {
                    // If ProcessMessage throws an exception, it will reappear in queue after visibility timeout
                    Trace.TraceError($"Processing Failed in ProcessQueueMessage. Exception: {ex.Message}, {ex.StackTrace}");
                }
            }
        }

        private static async Task ProcessMessage(CloudQueueMessage queueMessage, HttpClient httpClient, CloudTable cloudTable)
        {
            Trace.TraceInformation("Message processing");
            DataIngestion job = new DataIngestion(httpClient, azureTableProvider, cloudTable);
            try
            {
                await job.Execute(queueMessage.AsString);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Exception occured: {e.Message}");
                Trace.TraceError($"Exception occured at: {e.StackTrace}");
            }
        }
    }
}
