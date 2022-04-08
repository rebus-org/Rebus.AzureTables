using Azure.Data.Tables;
using Rebus.AzureTables.Sagas;
using Rebus.AzureTables.Sagas.Serialization;
using Rebus.Logging;
using Rebus.Sagas;
using Rebus.Tests.Contracts.Sagas;

namespace Rebus.AzureTables.Tests.Contracts
{
    public class AzureTablesSagaStorageFactory : ISagaStorageFactory
    {
        const string DataTableName = "RebusSagaData";
        static readonly string ConnectionString = TsTestConfig.ConnectionString;

        public AzureTablesSagaStorageFactory()
        {
            CleanUp();
        }

        public ISagaStorage GetSagaStorage()
        {
            var consoleLoggerFactory = new ConsoleLoggerFactory(true);
            var tableClient = new TableClient(ConnectionString, DataTableName);
            var factory = new TableClientFactory(tableClient);
            var storage = new TableStorageSagaStorage(factory, new DefaultSagaSerializer(), consoleLoggerFactory);
            tableClient.CreateIfNotExists();

            return storage;
        }

        public void CleanUp()
        {
            try
            {
                var tableClient = new TableClient(ConnectionString, DataTableName);
                tableClient.CreateIfNotExists();
                var result = tableClient.Query<TableEntity>();
                foreach (var item in result)
                {
                    tableClient.DeleteEntity(item.PartitionKey, item.RowKey);
                }
            }
            catch { }
        }
    }
}