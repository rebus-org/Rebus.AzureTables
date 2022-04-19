using Azure.Data.Tables;

namespace Rebus.AzureTables.Sagas
{
    public class AutoCreateConnectionStringTableClientFactory : AutoCreateTableClientFactory
    {
        public AutoCreateConnectionStringTableClientFactory(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected string ConnectionString { get; set; }

        public override TableClient CreateClient(string tableName) 
        {
            return new TableClient(ConnectionString, tableName);
        }
    }
}
