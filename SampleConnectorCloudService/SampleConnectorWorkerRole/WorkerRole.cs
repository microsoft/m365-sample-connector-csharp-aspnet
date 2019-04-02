// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.Azure;
    using System.Net.Http;
    using Microsoft.WindowsAzure.Storage.Table;
    using Microsoft.ApplicationInsights.Extensibility;

    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private AzureStorageQueueProvider queueProvider;
        private AzureTableProvider azureTableProvider;
        private CloudTable pageJobMappingTable;
        private static TimeSpan sleepTime = TimeSpan.FromSeconds(Convert.ToInt32(Settings.SleepTimeInSec));
        private static TimeSpan queueVisibilityTimeOutInSec = TimeSpan.FromSeconds(Convert.ToInt32(Settings.QueueVisibilityTimeOutInSec));

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            TelemetryConfiguration.Active.InstrumentationKey = Settings.APPINSIGHTS_INSTRUMENTATIONKEY;

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            if (bool.Parse(CloudConfigurationManager.GetSetting("DisableHttpsCheck")) == true)
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            InitializeStorageEntities().Wait();

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole has stopped");
        }

        private async Task InitializeStorageEntities()
        {
            queueProvider = new AzureStorageQueueProvider(CloudConfigurationManager.GetSetting("StorageAccountConnectionString"), Settings.QueueName);
            azureTableProvider = new AzureTableProvider(CloudConfigurationManager.GetSetting("StorageAccountConnectionString"));
            pageJobMappingTable = await azureTableProvider.EnsureTableExistAsync(Settings.PageJobMappingTableName);
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            HttpClient httpClient = new HttpClient();
            pageJobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);

            while (!cancellationToken.IsCancellationRequested)
            {
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
                        Trace.TraceError($"Processing Failed in RunAsync() WorkerRole. Exception: {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
        }

        private async Task ProcessMessage(CloudQueueMessage queueMessage, HttpClient httpClient, CloudTable cloudTable)
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
