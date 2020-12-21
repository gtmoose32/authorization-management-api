using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;

namespace AuthorizationManagement.Api.Extensions
{
    public static class CosmosClientBuilderExtensions
    {
        public static CosmosClientBuilder WithConnectionMode(this CosmosClientBuilder builder, string connectionMode)
        {
            if (!Enum.TryParse(connectionMode, true, out ConnectionMode mode))
                mode = ConnectionMode.Direct;

            return mode == ConnectionMode.Direct
                ? builder.WithConnectionModeDirect()
                : builder.WithConnectionModeGateway();
        }
    }
}
