using Azure.Data.Tables;
using Rebus.AzureTables.Sagas;
using Rebus.AzureTables.Sagas.Serialization;
using Rebus.Sagas;
using Rebus.Tests.Contracts.Sagas;

namespace Rebus.AzureTables.Tests.Contracts;

public class AzureTablesSagaStorageFactory : AzureTableFactoryBase, ISagaStorageFactory
{
    static readonly string ConnectionString = TsTestConfig.ConnectionString;

    public ISagaStorage GetSagaStorage()
    {
        var tableClient = new TableClient(ConnectionString, GetNextTableName());
        var factory = new TableClientFactory(tableClient);
        var storage = new TableStorageSagaStorage(factory, new DefaultSagaSerializer());

        tableClient.CreateIfNotExists();

        return storage;
    }

    public void CleanUp() => Dispose();
}