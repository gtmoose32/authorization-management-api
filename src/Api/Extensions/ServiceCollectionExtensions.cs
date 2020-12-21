using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthorizationManagement.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton(
                new CosmosClientBuilder(config[config["CosmosDb:ConnectionStringKey"]])
                    .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                    .WithApplicationRegion(Regions.NorthCentralUS)
                    .WithConnectionMode(config["CosmosDb:ConnectionMode"])
                    .Build());

            return services.AddSingleton(provider =>
                provider.GetRequiredService<CosmosClient>()
                    .GetContainer(config["CosmosDb:DatabaseId"], config["CosmosDb:ContainerId"]));
        }
    }
}