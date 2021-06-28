using NUnit.Framework;
using Rebus.Tests.Contracts.Sagas;

namespace Rebus.AzureTables.Tests.Contracts
{
    [TestFixture]
    public class TestSagaCorrelationSql : TestSagaCorrelation<AzureTablesSagaStorageFactory> { }
}