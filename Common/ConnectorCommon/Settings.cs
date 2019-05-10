// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Configuration;

    public class Settings
    {
        // Constant Settings
        public const string QueueName = "connectorqueue";

        public const string TokenTableName = "AccessTokensTable";

        public const string PageJobMappingTableName = "PageJobMappingTable";

        public const string ConfigurationSettingsTableName = "ConfigurationSettingsTable";

        public const string EventAPIBaseUrl = "https://webhook.ingestion.office.com/";

         public const string QueueVisibilityTimeOutInSec = "90";

        public const string SleepTimeInSec = "3";

        // Configurable Setting at Application Start Time 
        public static string TenantId = ConfigurationManager.AppSettings["TenantId"];

        public static string StorageAccountConnectionString = ConfigurationManager.AppSettings["StorageAccountConnectionString"];

        public static string APISecretKey = ConfigurationManager.AppSettings["APISecretKey"];


        // Configurable settings at run time

        public static string AAdAppId = string.Empty;

        public static string AAdAppSecret = string.Empty;

        public static string AADAppUri = string.Empty;

        public static string APPINSIGHTS_INSTRUMENTATIONKEY = string.Empty;

    }
}