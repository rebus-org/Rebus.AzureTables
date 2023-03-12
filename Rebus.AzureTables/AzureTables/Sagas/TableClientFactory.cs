using Azure.Data.Tables;
using System;
using System.Collections.Generic;

namespace Rebus.AzureTables.Sagas;

class TableClientFactory : ITableClientFactory
{
    public TableClientFactory(TableClient client)
    {
        Default = client;
    }

    protected TableClient Default { get; }
    protected Dictionary<Type, TableClient> TableClients = new();

    public TableClient GetTableClient(Type sagaDataType)
    {
        if (TableClients.TryGetValue(sagaDataType, out TableClient tableClient))
        {
            return tableClient;
        }
        return Default;
    }

    public void RegisterTableClient(Type sagaDataType, TableClient client)
    {
        TableClients.Add(sagaDataType, client);
    }
}