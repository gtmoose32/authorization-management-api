using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AuthorizationManagement.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration config)
        {
            if (!TryGetConnectionString(config, out var connectionString))
                throw new InvalidOperationException("Could not obtain a connection string from configuration.");

            var builder = new CosmosClientBuilder(connectionString)
                .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                .WithConnectionMode(config["CosmosDb:ConnectionMode"]);

            var regions = config.GetSection("CosmosDb:PreferredRegions").Get<string[]>();
            if (regions?.Length > 0)
                builder = builder.WithApplicationPreferredRegions(regions);

            services.AddSingleton(builder.Build());

            return services.AddSingleton(provider =>
                provider.GetRequiredService<CosmosClient>()
                    .GetContainer(config["CosmosDb:DatabaseId"], config["CosmosDb:ContainerId"]));
        }

        private static bool TryGetConnectionString(IConfiguration config, out string connectionString)
        {
            connectionString = config["CosmosDb:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(connectionString)) return true;

            var connectionStringPath = config["CosmosDb:ConnectionStringConfigKeyPath"];
            if (string.IsNullOrWhiteSpace(connectionStringPath)) return false;

            connectionString = config[connectionStringPath];
            return !string.IsNullOrWhiteSpace(connectionString);
        }

        private static CosmosClientBuilder WithConnectionMode(this CosmosClientBuilder builder, string connectionMode)
        {
            if (!Enum.TryParse(connectionMode, true, out ConnectionMode mode))
                mode = ConnectionMode.Direct;

            return mode == ConnectionMode.Direct
                ? builder.WithConnectionModeDirect()
                : builder.WithConnectionModeGateway();
        }
    }
}