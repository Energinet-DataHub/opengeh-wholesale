using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Extensions;

public static class ConfigurationRootExtensions
{
    /// <summary>
    /// Build configuration for loading settings from key vault secrets.
    /// </summary>
    public static IConfigurationRoot BuildSecretsConfiguration(this IConfigurationRoot root)
    {
        var sharedKeyVaultName = root.GetValue<string>("SHARED_KEYVAULT_NAME");
        var sharedKeyVaultUrl = $"https://{sharedKeyVaultName}.vault.azure.net/";

        var internalKeyVaultName = root.GetValue<string>("INTERNAL_KEYVAULT_NAME");
        var internalKeyVaultUrl = $"https://{internalKeyVaultName}.vault.azure.net/";

        return new ConfigurationBuilder()
            .AddAuthenticatedAzureKeyVault(sharedKeyVaultUrl)
            .AddAuthenticatedAzureKeyVault(internalKeyVaultUrl)
            .Build();
    }
}
