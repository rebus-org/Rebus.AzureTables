using Azure.Data.Tables;
using Rebus.AzureTables.Subscriptions;
using Rebus.Subscriptions;
using Rebus.Tests.Contracts.Subscriptions;

namespace Rebus.AzureTables.Tests.Contracts;

public class AzureTablesSubscriptionStorageFactory : AzureTableFactoryBase, ISubscriptionStorageFactory
{
    static readonly string ConnectionString = TsTestConfig.ConnectionString;

    public ISubscriptionStorage Create()
    {
        var subscriptionStorage = new AzureTablesSubscriptionStorage(CreateClient(), automaticallyCreateTable: true);
        subscriptionStorage.Initialize();
        return subscriptionStorage;
    }

    TableClient CreateClient() => new(ConnectionString, GetNextTableName());
    
    public void Cleanup() => Dispose();
}