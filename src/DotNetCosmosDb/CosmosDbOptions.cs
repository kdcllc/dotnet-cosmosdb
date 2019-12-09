using System;

namespace DotNetCosmosDb
{
    public class CosmosDbOptions
    {
        public Uri Endpoint { get; set; } = default!;

        public string AuthKey { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public string ContainerName { get; set; } = string.Empty;
    }
}
