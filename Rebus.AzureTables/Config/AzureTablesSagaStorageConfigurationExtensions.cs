using System;
using Azure;
using Azure.Core;
using Azure.Data.Tables;
using Rebus.AzureTables.Sagas;
using Rebus.AzureTables.Sagas.Serialization;
using Rebus.Logging;
using Rebus.Sagas;

namespace Rebus.Config
{
    /// <summary>
    /// Configuration extensions for Azure Tables
    /// </summary>
    public static class AzureTablesSagaStorageConfigurationExtensions
    {
        /// <summary>
        /// Configures Rebus to use Azure Storage Tables to store saga data
        /// </summary>
        public static void StoreInAzureTables(this StandardConfigurer<ISagaStorage> configurer, string connectionString, string tableName = "SagaData", bool automaticallyCreateTables = true)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            configurer.OtherService<ITableClientFactory>().Register(c => {
                if (automaticallyCreateTables)
                {
                    return new AutoCreateConnectionStringTableClientFactory(connectionString);
                }

                return new TableClientFactory(new TableClient(connectionString, tableName));
            });
            configurer.RegisterTableClient<ISagaData>(connectionString, tableName);
            configurer.StoreInAzureTables();
        }

        /// <summary>
        /// Configures Rebus to use Azure Storage Tables to store saga data
        /// </summary>
        public static void StoreInAzureTables(this StandardConfigurer<ISagaStorage> configurer, Uri endpoint, TokenCredential credentials, string tableName = "SagaData", bool automaticallyCreateTables = true)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            configurer.OtherService<ITableClientFactory>().Register(c => {
                if (automaticallyCreateTables)
                {
                    return new AutoCreateTokenCredentialTableClientFactory(endpoint, credentials);
                }

                return new TableClientFactory(new TableClient(endpoint, tableName, credentials));
            });
            configurer.StoreInAzureTables();
        }

        /// <summary>
        /// Configures Rebus to use Azure Storage Tables to store saga data
        /// </summary>
        public static void StoreInAzureTables(this StandardConfigurer<ISagaStorage> configurer, Uri endpoint, TableSharedKeyCredential credentials, string tableName = "SagaData", bool automaticallyCreateTables = true)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            configurer.OtherService<ITableClientFactory>().Register(c => {
                if (automaticallyCreateTables)
                {
                    return new AutoCreateTTableSharedKeyCredentialTableClientFactory(endpoint, credentials);
                }

                return new TableClientFactory(new TableClient(endpoint, tableName, credentials));
            });
            configurer.StoreInAzureTables();
        }

        /// <summary>
        /// Configures Rebus to use Azure Storage Tables to store saga data
        /// </summary>
        public static void StoreInAzureTables(this StandardConfigurer<ISagaStorage> configurer, Uri endpoint, AzureSasCredential credentials, bool automaticallyCreateTables = true)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));
            configurer.OtherService<ITableClientFactory>().Register(c =>  new TableClientFactory(new TableClient(endpoint, credentials)));
            configurer.StoreInAzureTables();
        }
        
        /// <summary>
        /// Configures saga to use your own custom saga serializer
        /// </summary>
        public static void RegisterTableClient<T>(this StandardConfigurer<ISagaStorage> configurer, TableClient client, bool automaticallyCreateTables = true) where T : ISagaData
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (client == null) throw new ArgumentNullException(nameof(client));

            configurer.OtherService<ITableClientFactory>().Decorate(c => {
                var factory = c.Get<ITableClientFactory>();

                if (automaticallyCreateTables)
                {
                    client.CreateIfNotExists();
                }

                factory.RegisterTableClient(typeof(T), client);
                return factory;
            });
        }

        /// <summary>
        /// Registers a TableClient for a SagaDataType
        /// </summary>
        public static void RegisterTableClient<T>(this StandardConfigurer<ISagaStorage> configurer, string connectionString, string tableName = "SagaData", bool automaticallyCreateTables = true) where T : ISagaData
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            configurer.RegisterTableClient<T>(new TableClient(connectionString, tableName), automaticallyCreateTables);
        }

        /// <summary>
        /// Registers a TableClient for a SagaDataType
        /// </summary>
        public static void RegisterTableClient<T>(this StandardConfigurer<ISagaStorage> configurer, Uri endpoint, TokenCredential credentials, string tableName = "SagaData", bool automaticallyCreateTables = true) where T: ISagaData
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            configurer.RegisterTableClient<T>(new TableClient(endpoint, tableName, credentials), automaticallyCreateTables);
        }

        /// <summary>
        /// Registers a TableClient for a SagaDataType
        /// </summary>
        public static void RegisterTableClient<T>(this StandardConfigurer<ISagaStorage> configurer, Uri endpoint, TableSharedKeyCredential credentials, string tableName = "SagaData", bool automaticallyCreateTables = true) where T : ISagaData
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            configurer.RegisterTableClient<T>(new TableClient(endpoint, tableName, credentials), automaticallyCreateTables);
        }

        /// <summary>
        /// Registers a TableClient for a SagaDataType
        /// </summary>
        public static void RegisterTableClient<T>(this StandardConfigurer<ISagaStorage> configurer, Uri endpoint, AzureSasCredential credentials, bool automaticallyCreateTables = true) where T : ISagaData
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));

            configurer.RegisterTableClient<T>(new TableClient(endpoint, credentials), automaticallyCreateTables);
        }

        /// <summary>
        /// Configures saga to use your own custom saga serializer
        /// </summary>
        public static void UseSagaSerializer(this StandardConfigurer<ISagaStorage> configurer, ISagaSerializer serializer = null)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));

            var serializerInstance = serializer ?? new DefaultSagaSerializer();

            configurer.OtherService<ISagaSerializer>().Decorate(c => serializerInstance);
        }

        static void StoreInAzureTables(this StandardConfigurer<ISagaStorage> configurer)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));

            configurer.Register(c =>
            {
                var rebusLoggerFactory = c.Get<IRebusLoggerFactory>();
                var serializer = c.Has<ISagaSerializer>(false) ? c.Get<ISagaSerializer>() : new DefaultSagaSerializer();
                var tableClientFactory = c.Get<ITableClientFactory>();
                var sagaStorage = new TableStorageSagaStorage(tableClientFactory, serializer, rebusLoggerFactory);

                return sagaStorage;
            });
        }
    }
}