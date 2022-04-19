using Azure.Core;
using Azure.Data.Tables;
using System;

namespace Rebus.AzureTables.Sagas
{
    public class AutoCreateTokenCredentialTableClientFactory : AutoCreateTableClientFactory
    {
        public AutoCreateTokenCredentialTableClientFactory(Uri endpoint, TokenCredential credential)
        {
            Endpoint = endpoint;
            Credential = credential;
        }

        protected Uri Endpoint { get; set; }
        public TokenCredential Credential { get; set; }

        public override TableClient CreateClient(string tableName) 
        {
            return new TableClient(Endpoint, tableName, Credential);
        }
    }
}
