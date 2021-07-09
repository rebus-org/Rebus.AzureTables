using Azure.Data.Tables;
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
            var tableClient = new TableClient(ConnectionString, DataTableName);
            var storage = new TableStorageSagaStorage(tableClient);

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