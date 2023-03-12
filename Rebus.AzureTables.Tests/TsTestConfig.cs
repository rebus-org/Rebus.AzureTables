using System;
using System.IO;

namespace Rebus.AzureTables.Tests;

static class TsTestConfig
{
    static readonly Lazy<string> LazyConnectionString = new(() =>
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "azure_storage_connection_string.txt");

        if (File.Exists(filePath))
        {
            return ConnectionStringFromFile(filePath);
        }

        const string variableName = "rebus2_ts_connection_string";
        var environmentVariable = Environment.GetEnvironmentVariable(variableName);

        if (!string.IsNullOrWhiteSpace(environmentVariable))
        {
            return ConnectionStringFromEnvironmentVariable(variableName);
        }

        Console.WriteLine($@"No connection string was found in file with path

    {filePath}

and the environment variable

    rebus2_ts_connection_string

was empty.

The local development storage will be used.");

        return "UseDevelopmentStorage=true";
    });

    static string GetConnectionString()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "azure_storage_connection_string.txt");

        if (File.Exists(filePath))
        {
            return ConnectionStringFromFile(filePath);
        }

        const string variableName = "rebus2_ts_connection_string";
        var environmentVariable = Environment.GetEnvironmentVariable(variableName);

        if (!string.IsNullOrWhiteSpace(environmentVariable)) return ConnectionStringFromEnvironmentVariable(variableName);

        return "UseDevelopmentStorage=true";

        throw new ApplicationException($@"Could not get Table Storage connection string. Tried to load from file

                {filePath}

            but the file did not exist. Also tried to get the environment variable named

                {variableName}

            but it was empty (or didn't exist).

            Please provide a connection string through one of the methods mentioned above.

            ");
    }

    public static string ConnectionString => LazyConnectionString.Value;

    static string ConnectionStringFromFile(string filePath)
    {
        Console.WriteLine("Using Table Storage connection string from file {0}", filePath);
        return File.ReadAllText(filePath);
    }

    static string ConnectionStringFromEnvironmentVariable(string environmentVariableName)
    {
        var value = Environment.GetEnvironmentVariable(environmentVariableName);

        Console.WriteLine("Using Table Storage connection string from env variable {0}", environmentVariableName);

        return value;
    }

}