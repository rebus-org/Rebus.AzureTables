using Azure.Data.Tables;
using Rebus.AzureTables.Sagas.Serialization;
using Rebus.Exceptions;
using Rebus.Logging;
using Rebus.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rebus.AzureTables.Sagas
{
    /// <summary>
    /// Implementation of <see cref="ISagaStorage"/> that uses the Azure Table Storage to store data
    /// </summary>
    public class TableStorageSagaStorage : ISagaStorage
    {
        private readonly ITableClientFactory _tableClientFactory;
        private readonly ISagaSerializer _sagaSerializer;
        private const string partitionKey = "Saga";
        private const string IdPropertyName = nameof(ISagaData.Id);
        private readonly ILog _log;

        /// <summary>
        /// Creates the saga storage
        /// </summary>
        /// <param name="tableClient">A TableClient</param>
        public TableStorageSagaStorage(ITableClientFactory tableClientFactory, ISagaSerializer sagaSerializer, IRebusLoggerFactory rebusLoggerFactory)
        {
            if (tableClientFactory == null) throw new ArgumentNullException(nameof(tableClientFactory));
            if (sagaSerializer == null) throw new ArgumentNullException(nameof(sagaSerializer));
            if (rebusLoggerFactory == null) throw new ArgumentNullException(nameof(rebusLoggerFactory));

            _tableClientFactory = tableClientFactory;
            _sagaSerializer = sagaSerializer;
            _log = rebusLoggerFactory.GetLogger<TableStorageSagaStorage>();
        }

        /// <summary>
        /// Removes the saga data instance from the index file
        /// </summary>
        public async Task Delete(ISagaData sagaData)
        {
            var currentRevision = sagaData.Revision;
            var currentData = await _tableClientFactory.GetTableClient(sagaData.GetType()).GetEntityAsync<TableEntity>(partitionKey, sagaData.Id.ToString(), new[] { "Revision", "ETag" });
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
            await _tableClientFactory.GetTableClient(sagaData.GetType()).DeleteEntityAsync(partitionKey, sagaData.Id.ToString());
        }

        /// <summary>
        /// Looks up an existing saga data instance from the index file
        /// </summary>
        public async Task<ISagaData> Find(Type sagaDataType, string propertyName, object propertyValue)
        {
            //await EnsureCreated();
            TableEntity entity = default;
            if (propertyName.Equals(IdPropertyName, StringComparison.InvariantCultureIgnoreCase))
            {
                string sagaId = propertyValue is string
                    ? (string)propertyValue
                    : ((Guid)propertyValue).ToString();

                entity = _tableClientFactory.GetTableClient(sagaDataType).Query<TableEntity>(filter: $"{nameof(TableEntity.RowKey)} eq '{propertyValue?.ToString() ?? ""}'", select: new[] { "SagaData" }).SingleOrDefault();

            }
            else
            {
                entity = _tableClientFactory.GetTableClient(sagaDataType).Query<TableEntity>(filter: $"{propertyName} eq '{propertyValue?.ToString() ?? ""}'", select: new[] { "SagaData" }).SingleOrDefault();
            }

            if (entity != null && entity.TryGetValue("SagaData", out object data))
            {
                try
                {
                    var sagaData = this._sagaSerializer.DeserializeFromString(sagaDataType, data.ToString());
                    return sagaData;
                }
                catch
                {
                }
            }

            return null;
        }

        /// <summary>
        /// Inserts the given saga data instance into the index file
        /// </summary>
        public async Task Insert(ISagaData sagaData, IEnumerable<ISagaCorrelationProperty> correlationProperties)
        {
            //await EnsureCreated();
            if (sagaData.Id == Guid.Empty)
            {
                throw new InvalidOperationException($"Saga data {sagaData.GetType()} has an uninitialized Id property!");
            }

            if (sagaData.Revision != 0)
            {
                throw new InvalidOperationException($"Attempted to insert saga data with ID {sagaData.Id} and revision {sagaData.Revision}, but revision must be 0 on first insert!");
            }

            /*var filter = correlationProperties
                .Select(p => p.PropertyName)
                .Select(path =>
                {
                    var value = GetPropertyValue(sagaData, path);

                    if (value == null)
                    {
                        return null;
                    }

                    return $"{path} eq '{value.ToString()}'";
                    //return new KeyValuePair<string, string>(path, value != null ? value.ToString() : null);
                }).Union(
                    new[] { $"SagaId eq '{sagaData.Id}'" }
                );

            var exists = tableClient.Query<TableEntity>(filter: string.Join(" or ", filter)).Any();
            if (exists) {
                throw new ConcurrencyException($"Saga data already exists!");
            }*/

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
                var currentData = await _tableClientFactory.GetTableClient(sagaData.GetType()).GetEntityAsync<TableEntity>(partitionKey, sagaData.Id.ToString(), new[] { "Revision", "ETag" });
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

        private TableEntity ToTableEntity(ISagaData sagaData, IEnumerable<ISagaCorrelationProperty> correlationProperties, string partitionKey = partitionKey)
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
        public static object GetPropertyValue(object obj, string path)
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

                    return new KeyValuePair<string, string>(path, value != null ? value.ToString() : null);
                })
                .Where(kvp => kvp.Value != null)
                .Union(new[] {
                    new KeyValuePair<string, string>("SagaId", sagaData.Id.ToString()),
                    new KeyValuePair<string, string>("Revision", sagaData.Revision.ToString())
                })
                .ToList();
        }
    }
}
