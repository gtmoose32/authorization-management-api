using AuthorizationManagement.Api.Controllers;
using AuthorizationManagement.Api.Models.Internal;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
// ReSharper disable PossibleNullReferenceException

namespace AuthorizationManagement.Api.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApplicationsControllerTests : ControllerTestBase
    {
        private ApplicationsController _sut;
        
        [TestInitialize]
        public void Init()
        {
            _sut = new ApplicationsController(Container, Mapper);
        }

        [TestMethod]
        public async Task GetAllAsync_Test()
        {
            //Arrange
            var apps = Fixture.CreateMany<Application>().ToArray();
            var mappedApp = Fixture.Create<Models.Application>();

            var feedIterator = CreateFeedIteratorWithResponse(apps);
            Container.GetItemQueryIterator<Application>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            Mapper.Map<Models.Application>(Arg.Any<Application>()).ReturnsForAnyArgs(mappedApp);
            
            //Act
            var result = await _sut.GetAllAsync().ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult) result;
            ok.Value.Should().NotBeNull();
            var results = (Models.Application[]) ok.Value;
            results.Should().NotBeNullOrEmpty();
            results.Length.Should().Be(apps.Length);
            results.All(a => ReferenceEquals(a, mappedApp)).Should().Be(true);

            Mapper.Received(apps.Length).Map<Models.Application>(Arg.Any<Application>());
            Container.Received(1)
                .GetItemQueryIterator<Application>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
        }
        
        [TestMethod]
        public async Task GetAsync_Test()
        {
            //Arrange
            const string id = "32";
            var app = Fixture.Build<Application>().With(a => a.Id, id).Create();
            var mappedApp = new Models.Application { Id = id, Name = app.Name };

            Container.ReadItemAsync<Application>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, app));

            Mapper.Map<Models.Application>(Arg.Any<Application>()).ReturnsForAnyArgs(mappedApp);
            
            //Act
            var result = await _sut.GetAsync(id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var appResult = ok.Value as Models.Application;
            appResult.Should().NotBeNull();

            Mapper.Received(1).Map<Models.Application>(Arg.Any<Application>());
            await Container.Received(1)
                .ReadItemAsync<Application>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task GetAsync_NotFound_Test()
        {
            //Arrange
            const string id = "32";

            Container.ReadItemAsync<Application>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            //Act
            var result = await _sut.GetAsync(id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();

            await Container.Received(1)
                .ReadItemAsync<Application>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);

            Mapper.DidNotReceiveWithAnyArgs().Map<Models.Application>(Arg.Any<Application>());
        }
        
        [TestMethod]
        public async Task PostAsync_Test()
        {
            //Arrange
            var app = Fixture.Create<Models.Application>();
            var mappedApp = new Application { Id = app.Id, Name = app.Name };

            Mapper.Map<Application>(Arg.Any<Models.Application>()).ReturnsForAnyArgs(mappedApp);

            Container.CreateItemAsync(Arg.Is(mappedApp), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, mappedApp));

            Mapper.Map<Models.Application>(Arg.Any<Application>()).Returns(app);
            
            //Act
            var result = await _sut.PostAsync(app).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var appResult = ok.Value as Models.Application;
            appResult.Should().NotBeNull();

            
            Mapper.Received(1).Map<Models.Application>(Arg.Any<Application>());
            await Container.Received(1)
                .CreateItemAsync(Arg.Is(mappedApp), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
            
            Mapper.Received(1).Map<Application>(Arg.Any<Models.Application>());
        }
    }
}
