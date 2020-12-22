using AuthorizationManagement.Api.Extensions;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace AuthorizationManagement.Api.Tests.Extensions
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
        private IServiceCollection _services;

        [TestInitialize]
        public void Init()
        {
            _services = new ServiceCollection();
        }

        [TestMethod]
        public void AddCosmosDb_Test()
        {
            //Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        {"CosmosDb:ConnectionString", "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="},
                        {"CosmosDb:DatabaseId", "db"},
                        {"CosmosDb:ContainerId", "col"}
                    })
                .Build();

            _services.AddCosmosDb(config);
            var provider = _services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
            
            //Act
            var result = provider.GetService<Container>();
            
            //Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void AddCosmosDb_AlternateConnectionStringConfig_Test()
        {
            //Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        {"CosmosDb:ConnectionStringConfigKeyPath", "Other:CosmosB:Config:Connection" },
                        {"Other:CosmosB:Config:Connection", "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="},
                        {"CosmosDb:DatabaseId", "db"},
                        {"CosmosDb:ContainerId", "col"}
                    })
                .Build();

            _services.AddCosmosDb(config);
            var provider = _services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

            //Act
            var result = provider.GetService<Container>();

            //Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void AddCosmosDb_MissingConnectionString_Test()
        {
            //Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            //Act
            Action act = () => _services.AddCosmosDb(config);

            //Assert
            act.Should()
                .ThrowExactly<InvalidOperationException>()
                .WithMessage("Could not obtain a connection string from configuration.");
        }

        [TestMethod]
        public void AddCosmosDb_WithPreferredRegions_Test()
        {
            //Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        {"CosmosDb:ConnectionString", "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="},
                        {"CosmosDb:DatabaseId", "db"},
                        {"CosmosDb:ContainerId", "col"}
                    })
                .SetBasePath(Path.Combine(AppContext.BaseDirectory, "Extensions"))
                .AddJsonFile("testsettings.json", false, false)
                .Build();

            _services.AddCosmosDb(config);
            var provider = _services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

            //Act
            var result = provider.GetService<Container>();

            //Assert
            result.Should().NotBeNull();
        }
    }
}