# Rebus.AzureTables

[![install from nuget](https://img.shields.io/nuget/v/Rebus.AzureTables.svg?style=flat-square)](https://www.nuget.org/packages/Rebus.AzureTables)

Provides Azure Storage Tables-based saga storage implementation for [Rebus](https://github.com/rebus-org/Rebus).

You can configure the table-based saga storage like this:

```csharp
var storageAccount = CloudStorageAccount.Parse(connectionString);

Configure.With(...)
	.(...)
	.Sagas(d => TODO)
	.Start();

```


```


![](https://raw.githubusercontent.com/rebus-org/Rebus/master/artwork/little_rebusbus2_copy-200x200.png)

---


