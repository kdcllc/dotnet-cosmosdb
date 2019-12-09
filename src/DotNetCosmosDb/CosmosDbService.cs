using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DotNetCosmosDb
{
    public class CosmosDbService
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        private readonly Container _container;
        private readonly ILogger<CosmosDbService> _logger;

        public CosmosDbService(Container container, ILogger<CosmosDbService> logger)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Delete(string query, CancellationToken cancellationToken)
        {
            var iterator = _container.GetItemQueryStreamIterator(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    MaxConcurrency = 1,
                    MaxItemCount = 200
                });

            var totalCount = 0;
            var deletedCount = 0;

            while (iterator.HasMoreResults)
            {
                var count = 0;
                using (var response = await iterator.ReadNextAsync(cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    count++;

                    dynamic streamResponse = FromStream<dynamic>(response.Content);
                    List<dynamic> logList = streamResponse.Documents.ToObject<List<dynamic>>();
                    totalCount += logList.Count;

                    foreach (var item in logList)
                    {
                        var id = (string)item.id;

                        var deleteResult = await _container.DeleteItemStreamAsync(id, new PartitionKey("dev2"));
                        if (deleteResult.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Deleted {id}", id);
                            deletedCount++;
                        }
                    }
                }
            }

            _logger.LogInformation("Total Count: {count}. Deleted {deletedCount}", totalCount, deletedCount);
        }

        private static T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        return Serializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }

        private static Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            using (var streamWriter = new StreamWriter(streamPayload, encoding: Encoding.Default, bufferSize: 1024, leaveOpen: true))
            {
                using (JsonWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Newtonsoft.Json.Formatting.None;
                    Serializer.Serialize(writer, input);
                    writer.Flush();
                    streamWriter.Flush();
                }
            }

            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}
