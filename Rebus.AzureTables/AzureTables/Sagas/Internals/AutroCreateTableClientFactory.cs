using System;
using System.Collections.Generic;
using Azure.Data.Tables;

namespace Rebus.AzureTables.Sagas.Internals;

abstract class AutoCreateTableClientFactory : ITableClientFactory
{
    protected readonly Dictionary<Type, TableClient> TableClients = new();

    public TableClient GetTableClient(Type sagaDataType)
    {
        if (TableClients.TryGetValue(sagaDataType, out TableClient tableClient)) {
            return tableClient;
        }
        tableClient = CreateClient(sagaDataType.Name);
        tableClient.CreateIfNotExists();
        RegisterTableClient(sagaDataType, tableClient);
        return tableClient;
    }

    public void RegisterTableClient(Type sagaDataType, TableClient client)
    {
        TableClients.Add(sagaDataType, client);
    }

    public abstract TableClient CreateClient(string tableName);
}