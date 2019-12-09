using Bet.AspNetCore.Options;

namespace DotNetCosmosDb
{
    public class HostingOptions
    {
        public AzureVaultOptionsEx? AzureVaultOptions { get; set; }

        public string Query { get; set; } = string.Empty;

        public CosmosDbOptions CosmosDbOptions { get; set; } = new CosmosDbOptions();
    }
}
