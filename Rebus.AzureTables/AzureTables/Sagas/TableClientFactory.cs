using Azure.Data.Tables;
using System;
using System.Collections.Generic;

namespace Rebus.AzureTables.Sagas
{
    public class TableClientFactory : ITableClientFactory
    {
        public TableClientFactory(TableClient client)
        {
            Default = client;
        }

        protected TableClient Default { get; }
        protected Dictionary<Type, TableClient> TableClients = new();

        public TableClient GetTableClient(Type SagaDataType)
        {
            if (TableClients.TryGetValue(SagaDataType, out TableClient tableClient))
            {
                return tableClient;
            }
            return Default;
        }

        public void RegisterTableClient(Type SagaDataType, TableClient client)
        {
            TableClients.Add(SagaDataType, client);
        }
    }
}
