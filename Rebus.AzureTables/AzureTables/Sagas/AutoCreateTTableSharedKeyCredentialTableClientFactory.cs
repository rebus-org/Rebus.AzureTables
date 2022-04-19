using Azure.Data.Tables;
using System;

namespace Rebus.AzureTables.Sagas
{
    public class AutoCreateTTableSharedKeyCredentialTableClientFactory : AutoCreateTableClientFactory
    {
        public AutoCreateTTableSharedKeyCredentialTableClientFactory(Uri endpoint, TableSharedKeyCredential credential)
        {
            Endpoint = endpoint;
            Credential = credential;
        }

        protected Uri Endpoint { get; }
        public TableSharedKeyCredential Credential { get; }

        public override TableClient CreateClient(string tableName) 
        {
            return new TableClient(Endpoint, tableName, Credential);
        }
    }
}
