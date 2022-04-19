using Azure.Data.Tables;

namespace Rebus.AzureTables.Sagas
{
    class AutoCreateConnectionStringTableClientFactory : AutoCreateTableClientFactory
    {
        public AutoCreateConnectionStringTableClientFactory(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected string ConnectionString { get; }

        public override TableClient CreateClient(string tableName)
        {
            return new TableClient(ConnectionString, tableName);
        }
    }
}
