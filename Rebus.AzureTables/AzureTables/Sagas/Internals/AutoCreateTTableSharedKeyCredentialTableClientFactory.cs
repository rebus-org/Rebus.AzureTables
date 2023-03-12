using System;
using Azure.Data.Tables;

namespace Rebus.AzureTables.Sagas.Internals;

class AutoCreateTTableSharedKeyCredentialTableClientFactory : AutoCreateTableClientFactory
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