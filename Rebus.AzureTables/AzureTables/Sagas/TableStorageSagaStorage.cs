using Azure.Data.Tables;
using Rebus.AzureTables.Sagas.Serialization;
using Rebus.Exceptions;
using Rebus.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable EmptyGeneralCatchClause

namespace Rebus.AzureTables.Sagas;

/// <summary>
/// Implementation of <see cref="ISagaStorage"/> that uses the Azure Table Storage to store data
/// </summary>
public class TableStorageSagaStorage : ISagaStorage
{
    private const string PartitionKey = "Saga";
    private const string IdPropertyName = nameof(ISagaData.Id);
    private readonly ITableClientFactory _tableClientFactory;
    private readonly ISagaSerializer _sagaSerializer;

    /// <summary>
    /// Creates the saga storage
    /// </summary>
    /// <param name="tableClientFactory">A TableClientFactory</param>
    /// <param name="sagaSerializer">The saga serializer to use</param>
    public TableStorageSagaStorage(ITableClientFactory tableClientFactory, ISagaSerializer sagaSerializer)
    {
        _tableClientFactory = tableClientFactory ?? throw new ArgumentNullException(nameof(tableClientFactory));
        _sagaSerializer = sagaSerializer ??throw new ArgumentNullException(nameof(sagaSerializer));
    }

    /// <summary>
    /// Removes the saga data instance from the index file
    /// </summary>
    public async Task Delete(ISagaData sagaData)
    {
        var currentRevision = sagaData.Revision;
        var currentData = await _tableClientFactory.GetTableClient(sagaData.GetType()).GetEntityAsync<TableEntity>(PartitionKey, sagaData.Id.ToString(), new[] { "Revision", "ETag" });
        if (currentData == null)
        {
            throw new ConcurrencyException($"Saga data with ID {sagaData.Id} does not exist!");
        }
        if (Int32.TryParse(currentData.Value.GetString("Revision"), out var storedRevision) && currentRevision != storedRevision)
        {
            throw new ConcurrencyException($"Attempted to update saga data with ID {sagaData.Id} with revision {sagaData.Revision}, but the existing data was updated to revision {currentData.Value.GetInt32("Revision")}");
        }
        sagaData.Revision++; // Needed to be compliant with the tests.
        //await EnsureCreated();
        await _tableClientFactory.GetTableClient(sagaData.GetType()).DeleteEntityAsync(PartitionKey, sagaData.Id.ToString());
    }

    /// <summary>
    /// Looks up an existing saga data instance from the index file
    /// </summary>
    public async Task<ISagaData> Find(Type sagaDataType, string propertyName, object propertyValue)
    {
        // use built-in table entity row key if we're querying by the saga data's ID
        var tableEntityPropertyKey = propertyName.Equals(IdPropertyName, StringComparison.InvariantCultureIgnoreCase)
            ? nameof(TableEntity.RowKey)
            : propertyName;

        var query = _tableClientFactory.GetTableClient(sagaDataType).QueryAsync<TableEntity>(
            filter: $"{tableEntityPropertyKey} eq '{propertyValue?.ToString() ?? ""}'",
            select: new[] { "SagaData" },
            maxPerPage: 1 //< no need to fetch more than this, ever
        );

        await using var asyncEnumerator = query.GetAsyncEnumerator();

        if (!await asyncEnumerator.MoveNextAsync()) return null;

        var entity = asyncEnumerator.Current;

        if (entity == null || !entity.TryGetValue("SagaData", out var data)) return null;

        try
        {
            return _sagaSerializer.DeserializeFromString(sagaDataType, data.ToString());
        }
        catch (Exception exception)
        {
            throw new FormatException($"Table entity {entity} could not be deserialized into {sagaDataType}",
                exception);
        }
    }

    /// <summary>
    /// Inserts the given saga data instance into the index file
    /// </summary>
    public async Task Insert(ISagaData sagaData, IEnumerable<ISagaCorrelationProperty> correlationProperties)
    {
        if (sagaData.Id == Guid.Empty)
        {
            throw new InvalidOperationException($"Saga data {sagaData.GetType()} has an uninitialized Id property!");
        }

        if (sagaData.Revision != 0)
        {
            throw new InvalidOperationException($"Attempted to insert saga data with ID {sagaData.Id} and revision {sagaData.Revision}, but revision must be 0 on first insert!");
        }

        var entity = ToTableEntity(sagaData, correlationProperties);

        await _tableClientFactory.GetTableClient(sagaData.GetType()).AddEntityAsync(entity);
    }

    /// <summary>
    /// Updates the given saga data instance in the index file
    /// </summary>
    public async Task Update(ISagaData sagaData, IEnumerable<ISagaCorrelationProperty> correlationProperties)
    {
        // await EnsureCreated();
        var currentRevision = sagaData.Revision;
        try
        {
            var currentData = await _tableClientFactory.GetTableClient(sagaData.GetType()).GetEntityAsync<TableEntity>(PartitionKey, sagaData.Id.ToString(), new[] { "Revision", "ETag" });
            if (currentData == null)
            {
                throw new ConcurrencyException($"Saga data with ID {sagaData.Id} does not exist!");
            }
            if (Int32.TryParse(currentData.Value.GetString("Revision"), out var storedRevision) && currentRevision != storedRevision)
            {
                throw new ConcurrencyException($"Attempted to update saga data with ID {sagaData.Id} with revision {sagaData.Revision}, but the existing data was updated to revision {storedRevision}");
            }
            // Increment Revision
            sagaData.Revision++;
            var entity = ToTableEntity(sagaData, correlationProperties);
            await _tableClientFactory.GetTableClient(sagaData.GetType()).UpdateEntityAsync(entity, currentData.Value.ETag, TableUpdateMode.Replace);
        }
        catch
        {
            // Reset revision
            sagaData.Revision = currentRevision;
            throw;
        }
    }

    private TableEntity ToTableEntity(ISagaData sagaData, IEnumerable<ISagaCorrelationProperty> correlationProperties, string partitionKey = PartitionKey)
    {
        var entity = new TableEntity(partitionKey, sagaData.Id.ToString());
        var indexedProperties = GetPropertiesToIndex(sagaData, correlationProperties);

        entity.Add("SagaData", _sagaSerializer.SerializeToString(sagaData));
        foreach (var indexedProperty in indexedProperties)
        {
            entity.Add(indexedProperty.Key, indexedProperty.Value);
        }

        return entity;
    }

    /// <summary>
    /// Gets a property value from an object using jsons '.' notation
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private static object GetPropertyValue(object obj, string path)
    {
        var dots = path.Split('.');

        foreach (var dot in dots)
        {
            var propertyInfo = obj.GetType().GetProperty(dot);
            if (propertyInfo == null) return null;
            obj = propertyInfo.GetValue(obj, new object[0]);
            if (obj == null) break;
        }

        return obj;
    }

    private static IEnumerable<KeyValuePair<string, string>> GetPropertiesToIndex(ISagaData sagaData, IEnumerable<ISagaCorrelationProperty> correlationProperties)
    {
        return correlationProperties
            .Select(p => p.PropertyName)
            .Select(path =>
            {
                var value = GetPropertyValue(sagaData, path);

                return new KeyValuePair<string, string>(path, value?.ToString());
            })
            .Where(kvp => kvp.Value != null)
            .Union(new[] {
                new KeyValuePair<string, string>("SagaId", sagaData.Id.ToString()),
                new KeyValuePair<string, string>("Revision", sagaData.Revision.ToString())
            })
            .ToList();
    }
}