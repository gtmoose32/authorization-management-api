using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationManagement.Api
{
    public static class Extensions
    {
        public static bool IsLocal(this IHostEnvironment environment) => environment.IsEnvironment("Local");

        public static CosmosClientBuilder WithConnectionMode(this CosmosClientBuilder builder, string connectionMode)
        {
            if (!Enum.TryParse(connectionMode, true, out ConnectionMode mode))
                mode = ConnectionMode.Direct;

            return mode == ConnectionMode.Direct 
                ? builder.WithConnectionModeDirect()
                : builder.WithConnectionModeGateway();
        }

        public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton(provider =>
                new CosmosClientBuilder(config[config["CosmosDb:ConnectionStringKey"]])
                    .WithSerializerOptions(new CosmosSerializationOptions {PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                    .WithApplicationRegion(Regions.NorthCentralUS)
                    .WithConnectionMode(config["CosmosDb:ConnectionMode"])
                    .Build());

            return services.AddSingleton(provider =>
                provider.GetRequiredService<CosmosClient>()
                    .GetContainer(config["CosmosDb:DatabaseId"], config["CosmosDb:ContainerId"]));
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this Container container, QueryDefinition query)
        {
            var results = await container.WhereAsync<T>(query, new QueryRequestOptions {MaxItemCount = 1})
                .ConfigureAwait(false);
            
            return results.SingleOrDefault();
        }

        public static async Task<IEnumerable<T>> WhereAsync<T>(this Container container, QueryDefinition query, QueryRequestOptions options = null)
        {
            var results = new List<T>();
            using var iterator = container.GetItemQueryIterator<T>(query, requestOptions: options);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync().ConfigureAwait(false);
                results.AddRange(response.Resource);
            }

            return results;
        }
    }
}
