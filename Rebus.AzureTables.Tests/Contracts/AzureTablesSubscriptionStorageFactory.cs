using System.Collections.Generic;
using Azure.Data.Tables;
using Rebus.AzureTables.Subscriptions;
using Rebus.Subscriptions;
using Rebus.Tests.Contracts.Subscriptions;

namespace Rebus.AzureTables.Tests.Contracts
{
    public class AzureTablesSubscriptionStorageFactory : ISubscriptionStorageFactory
    {
        const string DataTableName = "RebusSubscriptionData";
        static readonly string ConnectionString = TsTestConfig.ConnectionString;

        public AzureTablesSubscriptionStorageFactory() => CreateClient().CreateIfNotExists();

        public ISubscriptionStorage Create() => new AzureTablesSubscriptionStorage(CreateClient());

        public void Cleanup()
        {
            // Cannot drop the table as creation of the table in subsequent tests would fail with 429 Conflict (TableBeingDeleted) for few minutes. Clearing the table instead.
            var client = CreateClient();
            var subscriptionsToDelete = new List<TableEntity>();
            foreach (var entitity in client.Query<TableEntity>(select: new[] { "PartitionKey", "RowKey" }))
                subscriptionsToDelete.Add(entitity);
            foreach (var entity in subscriptionsToDelete)
                client.DeleteEntity(entity.PartitionKey, entity.RowKey);
        }

        private TableClient CreateClient() => new TableClient(ConnectionString, DataTableName);
    }
}