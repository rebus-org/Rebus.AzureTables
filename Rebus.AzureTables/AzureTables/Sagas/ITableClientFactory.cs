using Azure.Data.Tables;
using System;

namespace Rebus.AzureTables.Sagas
{
    public interface ITableClientFactory
    {
        void RegisterTableClient(Type SagaDataType, TableClient client);
        TableClient GetTableClient(Type SagaDataType);
    }
}
