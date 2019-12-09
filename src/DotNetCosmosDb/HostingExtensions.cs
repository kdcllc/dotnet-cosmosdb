using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetCosmosDb
{
    public static class HostingExtensions
    {
        public static async Task<int> RunAync(string[] args, HostingOptions hostingOptions, CancellationToken cancellation)
        {
            using var host = CreateHostBuilder(args, hostingOptions).UseConsoleLifetime().Build();

            await host.StartAsync();

            using var scope = host.Services.CreateScope();
            var hostAppCanellationToken = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation, hostAppCanellationToken.ApplicationStopping);

            var job = scope.ServiceProvider.GetRequiredService<CosmosDbService>();

            await job.Delete(hostingOptions.Query, cts.Token);

            await host.StopAsync();
            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args, HostingOptions hostingOptions)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    IConfigurationRoot? configuration = null;

                    if (hostingOptions.AzureVaultOptions != null
                        && !string.IsNullOrEmpty(hostingOptions.AzureVaultOptions.BaseUrl))
                    {
                        var dic = new Dictionary<string, string>
                        {
                            { $"{hostingOptions.AzureVaultOptions.SectionName}:BaseUrl", $"{hostingOptions.AzureVaultOptions.BaseUrl}" },
                            { $"{hostingOptions.AzureVaultOptions.SectionName}:ClientId", $"{hostingOptions.AzureVaultOptions.ClientId}" },
                            { $"{hostingOptions.AzureVaultOptions.SectionName}:ClientSecret", $"{hostingOptions.AzureVaultOptions.ClientSecret}" },
                        };

                        configBuilder.AddInMemoryCollection(dic);

                        // based on environment Development = dev; Production = prod prefix in Azure Vault.
                        var envName = hostingContext.HostingEnvironment.EnvironmentName;

                        configuration = configBuilder.AddAzureKeyVault(
                            hostingEnviromentName: envName,
                            usePrefix: false,
                            reloadInterval: TimeSpan.FromSeconds(30));
                    }

                    // helpful to see what was retrieved from all of the configuration providers.
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        (configuration ?? (configuration = configBuilder.Build())).DebugConfigurations();
                    }
                })
                .ConfigureLogging((hostingContext, logger) =>
                {
                    logger.AddConfiguration(hostingContext.Configuration);
                    logger.AddConsole();
                    logger.AddDebug();
                })
                .ConfigureServices(services =>
                {
                    services.AddCosmosDb();
                    services.AddSingleton<CosmosDbService>();
                });
        }
    }
}
