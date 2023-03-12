using System;
using Azure.Core;
using Azure.Data.Tables;
using Rebus.AzureTables.Subscriptions;
using Rebus.Subscriptions;
// ReSharper disable UnusedMember.Global

namespace Rebus.Config;

/// <summary>
/// Configuration extensions for subcriptions storage in Azure Cosmos Db Tables or Azure Table Storage
/// </summary>
public static class AzureTablesSubscriptionConfigurationExtensions
{
    /// <summary>
    /// Configure Rebus to store subscriptions in Azure Cosmos Db Tables or Azure Table Storage
    /// </summary>
    /// <param name="configurer"></param>
    /// <param name="connectionString">Azure Cosmos Db Tables or Azure Table Storage connection string including key</param>
    /// <param name="tableName">Name of the table where the subscriptions will be stored</param>
    /// <param name="isCentralized">True if this subscription storage is centralized (i.e. if subscribers can register themselves directly)</param>
    /// <param name="automaticallyCreateTable">True if the storage table should be created if it does not exist</param>
    /// <exception cref="ArgumentNullException">Thrown when not all mandatory parameters are provided</exception>
    public static void StoreInAzureTables(this StandardConfigurer<ISubscriptionStorage> configurer, string connectionString, string tableName = "RebusSubscriptions", bool isCentralized = false, bool automaticallyCreateTable = false)
    {
        if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

        configurer.StoreInAzureTables(new TableServiceClient(connectionString), tableName, isCentralized, automaticallyCreateTable);
    }

    /// <summary>
    /// Configure Rebus to store subscriptions in Azure Cosmos Db Tables or Azure Table Storage
    /// </summary>
    /// <param name="configurer"></param>
    /// <param name="serviceEndpoint">Azure Cosmos Db Tables or Azure Table Storage service endpoint</param>
    /// <param name="credential">Credetial for authentication to Azure service (eg. new DefaultAzureCredential())</param>
    /// <param name="tableName">Name of the table where the subscriptions will be stored </param>
    /// <param name="isCentralized">True if this subscription storage is centralized (i.e. if subscribers can register themselves directly)</param>
    /// <param name="automaticallyCreateTable">True if the storage table should be created if not exist</param>
    /// <exception cref="ArgumentNullException">Thrown when not all mandatory parameters are provided</exception>
    public static void StoreInAzureTables(this StandardConfigurer<ISubscriptionStorage> configurer, Uri serviceEndpoint, TokenCredential credential, string tableName = "RebusSubscriptions", bool isCentralized = false, bool automaticallyCreateTable = false)
    {
        if (serviceEndpoint == null) throw new ArgumentNullException(nameof(serviceEndpoint));
        if (credential == null) throw new ArgumentNullException(nameof(credential));

        configurer.StoreInAzureTables(new TableServiceClient(serviceEndpoint, credential), tableName, isCentralized, automaticallyCreateTable);
    }

    /// <summary>
    /// Configure Rebus to store subscriptions in Azure Cosmos Db Tables or Azure Table Storage
    /// </summary>
    /// <param name="configurer"></param>
    /// <param name="tableServiceClient">Azure Cosmos Db Tables or Azure Table Storage TableServiceClient</param>
    /// <param name="tableName">Name of the table where the subscriptions will be stored </param>
    /// <param name="isCentralized">True if this subscription storage is centralized (i.e. if subscribers can register themselves directly)</param>
    /// <param name="automaticallyCreateTable">True if the storage table should be created if not exist</param>
    /// <exception cref="ArgumentNullException">Thrown when not all mandatory parameters are provided</exception>
    public static void StoreInAzureTables(this StandardConfigurer<ISubscriptionStorage> configurer, TableServiceClient tableServiceClient, string tableName = "RebusSubscriptions", bool isCentralized = false, bool automaticallyCreateTable = false)
    {
        if (tableServiceClient == null) throw new ArgumentNullException(nameof(tableServiceClient));
        if (tableName == null) throw new ArgumentNullException(nameof(tableName));

        configurer.StoreInAzureTables(tableServiceClient.GetTableClient(tableName), isCentralized, automaticallyCreateTable);
    }

    /// <summary>
    /// Configure Rebus to store subscriptions in Azure Cosmos Db Tables or Azure Table Storage
    /// </summary>
    /// <param name="configurer"></param>
    /// <param name="tableClient">Azure Cosmos Db Tables or Azure Table Storage TableClient</param>
    /// <param name="isCentralized">True if this subscription storage is centralized (i.e. if subscribers can register themselves directly)</param>
    /// <param name="automaticallyCreateTable">True if the storage table should be created if not exist</param>
    /// <exception cref="ArgumentNullException">Thrown when not all mandatory parameters are provided</exception>
    public static void StoreInAzureTables(this StandardConfigurer<ISubscriptionStorage> configurer, TableClient tableClient, bool isCentralized = false, bool automaticallyCreateTable = false)
    {
        if (configurer == null) throw new ArgumentNullException(nameof(configurer));
        if (tableClient == null) throw new ArgumentNullException(nameof(tableClient));

        configurer.Register(_ => new AzureTablesSubscriptionStorage(tableClient, isCentralized, automaticallyCreateTable));
    }
}