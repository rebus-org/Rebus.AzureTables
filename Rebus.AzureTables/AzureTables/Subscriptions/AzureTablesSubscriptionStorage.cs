using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.Subscriptions;

namespace Rebus.AzureTables.Subscriptions;
/// <summary>
/// Implementation of <see cref="ISubscriptionStorage"/> that uses Azure tables (either Azure Cosmos DB Table Api or Azure Table Storage) to store subscriptions
/// </summary>
public class AzureTablesSubscriptionStorage : ISubscriptionStorage, IInitializable
{
    readonly TableClient _tableClient;
    readonly bool _automaticallyCreateTable;

    /// <summary>
    /// Creates the subscription storage
    /// </summary>
    public AzureTablesSubscriptionStorage(TableClient tableClient, bool isCentralized = false, bool automaticallyCreateTable = false)
    {
        _tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
        _automaticallyCreateTable = automaticallyCreateTable;
        IsCentralized = isCentralized;
    }

    /// <summary>
    /// Initializes the subscription storage by ensuring that the necessary table is created
    /// </summary>
    public void Initialize()
    {
        if (!_automaticallyCreateTable) return;

        _tableClient.CreateIfNotExists();
    }

    /// <summary>
    /// Gets all subscribers by getting row IDs from the partition named after the given <paramref name="topic"/>
    /// </summary>
    public async Task<string[]> GetSubscriberAddresses(string topic)
    {
        try
        {
            var items = _tableClient.QueryAsync<TableEntity>(s => s.PartitionKey == topic, select: new[] { "PartitionKey", "RowKey" });
            var addresses = new List<string>();
            await foreach (var address in items)
            {
                addresses.Add(address.RowKey);
            }
            return addresses.ToArray();
        }
        catch (RequestFailedException ex)
        {
            throw new RebusApplicationException(ex, $"Could not get subscriber addresses for '{topic}'");
        }
    }

    /// <summary>
    /// Registers the given <paramref name="subscriberAddress"/> as a subscriber of the topic named <paramref name="topic"/>
    /// by inserting a row with the address as the row ID under a partition key named after the topic
    /// </summary>
    public async Task RegisterSubscriber(string topic, string subscriberAddress)
    {
        try
        {
            await _tableClient.UpsertEntityAsync(new TableEntity(topic, subscriberAddress), mode: TableUpdateMode.Replace);
        }
        catch (RequestFailedException ex)
        {
            throw new RebusApplicationException(ex, $"Could not subscribe {subscriberAddress} to '{topic}'");
        }
    }

    /// <summary>
    /// Unregisters the given <paramref name="subscriberAddress"/> as a subscriber of the topic named <paramref name="topic"/>
    /// by removing the row with the address as the row ID under a partition key named after the topic
    /// </summary>
    public async Task UnregisterSubscriber(string topic, string subscriberAddress)
    {
        try
        {
            await _tableClient.DeleteEntityAsync(topic, subscriberAddress);
        }
        catch (RequestFailedException ex)
        {
            throw new RebusApplicationException(ex, $"Could not unsubscribe {subscriberAddress} from '{topic}'");
        }
    }

    /// <summary>
    /// Gets whether this subscription storage is centralized (i.e. whether subscribers can register themselves directly)
    /// </summary>
    public bool IsCentralized { get; }
}