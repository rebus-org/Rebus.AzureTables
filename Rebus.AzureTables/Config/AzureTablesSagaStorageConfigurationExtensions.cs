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


            configurer.Register(c =>
            {
                var rebusLoggerFactory = c.Get<IRebusLoggerFactory>();
                var serializer = c.Has<ISagaSerializer>(false) ? c.Get<ISagaSerializer>() : new DefaultSagaSerializer();
                var sagaStorage = new TableStorageSagaStorage(new TableClient(connectionString, tableName), serializer, rebusLoggerFactory);

                if (automaticallyCreateTables) {
                    sagaStorage.EnsureCreated().GetAwaiter().GetResult();
                }

                return sagaStorage;
            });
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

            configurer.Register(c =>
            {
                var rebusLoggerFactory = c.Get<IRebusLoggerFactory>();
                var serializer = c.Has<ISagaSerializer>(false) ? c.Get<ISagaSerializer>() : new DefaultSagaSerializer();
                var sagaStorage = new TableStorageSagaStorage(new TableClient(endpoint, tableName, credentials), serializer, rebusLoggerFactory);

                if (automaticallyCreateTables)
                {
                    sagaStorage.EnsureCreated().GetAwaiter().GetResult();
                }

                return sagaStorage;
            });
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

            configurer.Register(c =>
            {
                var rebusLoggerFactory = c.Get<IRebusLoggerFactory>();
                var serializer = c.Has<ISagaSerializer>(false) ? c.Get<ISagaSerializer>() : new DefaultSagaSerializer();
                var sagaStorage = new TableStorageSagaStorage(new TableClient(endpoint, tableName, credentials), serializer, rebusLoggerFactory);

                if (automaticallyCreateTables)
                {
                    sagaStorage.EnsureCreated().GetAwaiter().GetResult();
                }

                return sagaStorage;
            });
        }

        /// <summary>
        /// Configures Rebus to use Azure Storage Tables to store saga data
        /// </summary>
        public static void StoreInAzureTables(this StandardConfigurer<ISagaStorage> configurer, Uri endpoint, AzureSasCredential credentials, bool automaticallyCreateTables = true)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));

            configurer.Register(c =>
            {
                var rebusLoggerFactory = c.Get<IRebusLoggerFactory>();
                var serializer = c.Has<ISagaSerializer>(false) ? c.Get<ISagaSerializer>() : new DefaultSagaSerializer();
                var sagaStorage = new TableStorageSagaStorage(new TableClient(endpoint, credentials), serializer, rebusLoggerFactory );

                if (automaticallyCreateTables)
                {
                    sagaStorage.EnsureCreated().GetAwaiter().GetResult();
                }

                return sagaStorage;
            });
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
    }
}