using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCosmosDb
{
    internal sealed class Program
    {
        internal static async Task<int> Main(string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                Description = "CosmosDb dotnet cli tool",
            };

            // azure url
            var azureOption = new Option(new string[] { "--azure", "-a" }, "The Azure Key Vault")
            {
                Argument = new Argument<string>((SymbolResult result, out string value) =>
                {
                    value = result.Token.Value;

                    return true;
                })
            };

            rootCommand.AddOption(azureOption);

            // query option
            var queryOption = new Option(new string[] { "--query", "-q" }, "The query to be used for delete.")
            {
                Argument = new Argument<string>()
            };
            rootCommand.AddOption(queryOption);

            // endpoint
            var endPointOption = new Option(new string[] { "--endpoint", "-e" }, "The CosmosDb endpoint.")
            {
                Argument = new Argument<Uri>()
            };
            rootCommand.AddOption(endPointOption);

            var dbNameOption = new Option(new string[] { "--name", "-n" }, "The CosmosDb Name.")
            {
                Argument = new Argument<string>()
            };
            rootCommand.AddOption(dbNameOption);

            var containerOption = new Option(new string[] { "--container", "-c" }, "The CosmosDb Container.")
            {
                Argument = new Argument<string>()
            };
            rootCommand.AddOption(containerOption);

            var dbKeyOption = new Option(new string[] { "--key", "-k" }, "The CosmosDb Key.")
            {
                Argument = new Argument<string>()
            };
            rootCommand.AddOption(dbKeyOption);

            var deleteCommand = new Command("del", "Deletes CosmosDb entities based on a query")
            {
               Handler = CommandHandler.Create<string, string, Uri, string, string, IConsole, CancellationToken>(async (
                  azure,
                  query,
                  endpoint,
                  name,
                  key,
                  console,
                  cancellationToken) =>
               {
                   console.Out.WriteLine(azure);

                   var hostingOptions = new HostingOptions
                   {
                       AzureVaultOptions = new AzureVaultOptionsEx { BaseUrl = azure },
                       Query = query,
                       CosmosDbOptions = new CosmosDbOptions
                       {
                            DatabaseName = name,
                            ContainerName = name,
                            Endpoint = endpoint,
                            AuthKey = key
                       }
                   };

                   return await HostingExtensions.RunAync(args, hostingOptions, cancellationToken);
               })
            };

            rootCommand.AddCommand(deleteCommand);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
