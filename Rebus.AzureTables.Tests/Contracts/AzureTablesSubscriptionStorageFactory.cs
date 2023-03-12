using System;
using System.Collections.Concurrent;
using System.IO;
using Azure.Data.Tables;
using Rebus.AzureTables.Subscriptions;
using Rebus.Subscriptions;
using Rebus.Tests.Contracts;
using Rebus.Tests.Contracts.Subscriptions;

namespace Rebus.AzureTables.Tests.Contracts;

public class AzureTablesSubscriptionStorageFactory : ISubscriptionStorageFactory
{
    static readonly string ConnectionString = TsTestConfig.ConnectionString;

    readonly ConcurrentStack<IDisposable> _disposables = new();

    public ISubscriptionStorage Create()
    {
        var subscriptionStorage = new AzureTablesSubscriptionStorage(CreateClient(), automaticallyCreateTable: true);
        subscriptionStorage.Initialize();
        return subscriptionStorage;
    }

    int counter = 1;

    TableClient CreateClient()
    {
        var tableName = $"table{counter++:00000}";

        _disposables.Push(new TableDeleter(tableName));

        return new TableClient(ConnectionString, tableName);
    }

    public void Cleanup()
    {
        while (_disposables.TryPop(out var disposable))
        {
            disposable.Dispose();
        }
    }
}