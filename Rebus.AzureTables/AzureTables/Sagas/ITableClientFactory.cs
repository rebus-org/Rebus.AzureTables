using Azure.Data.Tables;
using System;

namespace Rebus.AzureTables.Sagas;

/// <summary>
/// Factory for table clients.
/// </summary>
public interface ITableClientFactory
{
    /// <summary>
    /// Registers a <see cref="TableClient"/> for the given <paramref name="sagaDataType"/>
    /// </summary>
    void RegisterTableClient(Type sagaDataType, TableClient client);
        
    /// <summary>
    /// Gets the table client to use for the given <paramref name="sagaDataType"/>
    /// </summary>
    TableClient GetTableClient(Type sagaDataType);
}