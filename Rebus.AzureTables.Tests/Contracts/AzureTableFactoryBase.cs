using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Rebus.AzureTables.Tests.Contracts;

public abstract class AzureTableFactoryBase : IDisposable
{
    static int _counter;

    readonly ConcurrentStack<IDisposable> _disposables = new();

    protected string GetNextTableName()
    {
        var tableName = $"table{Interlocked.Increment(ref _counter):00000}";
        _disposables.Push(new TableDeleter(tableName));
        return tableName;
    }

    public void Dispose()
    {
        while (_disposables.TryPop(out var disposable))
        {
            disposable.Dispose();
        }
    }
}