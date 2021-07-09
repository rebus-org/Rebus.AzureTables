using System;
using Rebus.Sagas;

namespace Rebus.Config
{
    /// <summary>
    /// Configuration extensions for Azure Tables
    /// </summary>
    public static class AzureTablesSagaStorageConfigurationExtensions
    {
        /// <summary>
        /// Configures Rebus to use Azure Storage Tables to store saga data
        /// </summary>
        public static void StoreInAzureTables(this StandardConfigurer<ISagaStorage> configurer)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
        }
    }
}