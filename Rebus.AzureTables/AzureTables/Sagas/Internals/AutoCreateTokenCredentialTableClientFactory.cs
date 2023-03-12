﻿using System;
using Azure.Core;
using Azure.Data.Tables;

namespace Rebus.AzureTables.Sagas.Internals;

class AutoCreateTokenCredentialTableClientFactory : AutoCreateTableClientFactory
{
    public AutoCreateTokenCredentialTableClientFactory(Uri endpoint, TokenCredential credential)
    {
        Endpoint = endpoint;
        Credential = credential;
    }

    protected Uri Endpoint { get; }
    public TokenCredential Credential { get; }

    public override TableClient CreateClient(string tableName) 
    {
        return new TableClient(Endpoint, tableName, Credential);
    }
}