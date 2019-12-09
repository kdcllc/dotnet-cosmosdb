using Bet.AspNetCore.Options;

namespace DotNetCosmosDb
{
    public class AzureVaultOptionsEx : AzureVaultOptions
    {
        public string SectionName { get; set; } = "AzureVault";
    }
}
