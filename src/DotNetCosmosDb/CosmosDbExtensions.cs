using System;

using DotNetCosmosDb;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CosmosDbExtensions
    {
        /// <summary>
        /// Add Azure CosmosDb options and <see cref="IDocumentClient"/> registration.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="sectionName">The configuration section name. The default is 'CosmosDb'.</param>
        /// <param name="action">The configuration options for <see cref="CosmosDbOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddCosmosDb(
            this IServiceCollection services,
            string sectionName = "CosmosDb",
            Action<CosmosDbOptions>? action = null)
        {
            services.AddTransient<IConfigureOptions<CosmosDbOptions>>(sp =>
            {
                return new ConfigureOptions<CosmosDbOptions>((options) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    configuration.Bind(sectionName, options);

                    action?.Invoke(options);
                });
            });

            services.AddSingleton(sp =>
            {
                var dbOptions = sp.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

                var client = new CosmosClient(dbOptions.Endpoint.AbsoluteUri, dbOptions.AuthKey);

                if (string.IsNullOrEmpty(dbOptions.ContainerName))
                {
                    dbOptions.ContainerName = dbOptions.DatabaseName;
                }

                return client.GetContainer(dbOptions.DatabaseName, dbOptions.ContainerName);
            });

            return services;
        }
    }
}
