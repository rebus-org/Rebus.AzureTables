# Rebus.AzureTables

[![install from nuget](https://img.shields.io/nuget/v/Rebus.AzureTables.svg?style=flat-square)](https://www.nuget.org/packages/Rebus.AzureTables)

Provides Azure Storage Tables-based saga storage implementation for [Rebus](https://github.com/rebus-org/Rebus).

You can configure the table-based saga storage like this:

```csharp

Configure.With(...)
	.(...)
	.Sagas(d => d.StoreInAzureTables(storageConnectionString, "tableName"))
	.Start();


Configure.With(...)
	.(...)
	.Sagas(d => d.StoreInAzureTables(endpoint, new DefaultAzureCredential(includeInteractiveCredentials: true), "tableName"))
	.Start();

```

If you want to add different tables for each SagaDataType you can let the framework handles this. (create of tables by the process must be possible)

```csharp

Configure.With(...)
	.(...)
	.Sagas(d => d.StoreInAzureTables(storageConnectionString, automaticallyGenerateClients: true))
	.Start();


```

Or you can define them your self. The tableName passed in the StoreInAzureTables servers as default table (used when no tableClient for the SagaDataType is found.)

```csharp

Configure.With(...)
	.(...)
	.Sagas(d => {
		d.StoreInAzureTables(storageConnectionString, "default");
		d.RegisterTableClient<SagaData>(storageConnectionString, "tableName");
		d.RegisterTableClient<SagaData2>(storageConnectionString, "tableName2");
	 })
	.Start();


```

![](https://raw.githubusercontent.com/rebus-org/Rebus/master/artwork/little_rebusbus2_copy-200x200.png)

---

