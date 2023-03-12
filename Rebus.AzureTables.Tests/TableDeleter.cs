using System;
using Azure.Data.Tables;

namespace Rebus.AzureTables.Tests;

class TableDeleter : IDisposable
{
    readonly string _tableName;

    public TableDeleter(string tableName)
    {
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    }

    public void Dispose() => new TableClient(TsTestConfig.ConnectionString, _tableName).Delete();
}