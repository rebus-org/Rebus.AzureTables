using Rebus.Sagas;
using Rebus.Tests.Contracts.Sagas;

namespace Rebus.AzureTables.Tests.Contracts
{
    public class AzureTablesSagaStorageFactory : ISagaStorageFactory
    {
        public ISagaStorage GetSagaStorage()
        {
            throw new System.NotImplementedException();
        }

        public void CleanUp()
        {
            throw new System.NotImplementedException();
        }
    }
}