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
using User = AuthorizationManagement.Api.Models.Internal.User;

// ReSharper disable PossibleNullReferenceException

namespace AuthorizationManagement.Api.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class UsersControllerTests : ControllerTestBase
    {
        private string _id;
        private UsersController _sut;
        
        [TestInitialize]
        public void Init()
        {
            _id = Fixture.Create<string>();
            _sut = new UsersController(Container, Mapper);
        }
        
        [TestMethod]
        public async Task GetAllAsync_Test()
        {
            //Arrange
            var users = Fixture.CreateMany<User>().ToArray();
            var mappedUser = Fixture.Create<Models.UserInfo>();

            var feedIterator = CreateFeedIteratorWithResponse(users);
            Container.GetItemQueryIterator<User>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            Mapper.Map<Models.UserInfo>(Arg.Any<Models.User>()).ReturnsForAnyArgs(mappedUser);
            
            //Act
            var result = await _sut.GetAllAsync(ApplicationId).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult) result;
            ok.Value.Should().NotBeNull();
            var results = (Models.UserInfo[]) ok.Value;
            results.Should().NotBeNullOrEmpty();
            results.Length.Should().Be(users.Length);
            results.All(a => ReferenceEquals(a, mappedUser)).Should().Be(true);

            Mapper.Received(users.Length).Map<Models.UserInfo>(Arg.Any<User>());

            Container.Received(1)
                .GetItemQueryIterator<User>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
        }
        
        [TestMethod]
        public async Task GetAsync_Test()
        {
            //Arrange
            var user = Fixture.Build<Models.User>()
                .With(u => u.Id, _id)
                .Without(u => u.Groups)
                .Create();
            
            var groups = Fixture.CreateMany<Group>().ToArray();
            var groupDtos = groups.Select(g => new Models.Group {Id = g.Id, Name = g.Name}).ToList();
            var document = Fixture.Build<User>().With(a => a.Id, _id).Create();

            Container.ReadItemAsync<User>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, document));
            
            var feedIterator = CreateFeedIteratorWithResponse(groups);
            Container.GetItemQueryIterator<Group>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            Mapper.Map<Models.User>(Arg.Any<User>()).ReturnsForAnyArgs(user);

            Mapper.Map<Models.Group>(Arg.Any<Group>()).Returns(info => groupDtos.Single(g=> g.Id == info.Arg<Group>().Id));
            
            //Act
            var result = await _sut.GetAsync(ApplicationId, _id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var userResult = ok.Value as Models.User;
            userResult.Should().NotBeNull();
            userResult.Groups.Should().NotBeNullOrEmpty();
            userResult.Groups.Count.Should().Be(groups.Length);

            Mapper.Received(1).Map<Models.User>(Arg.Is(document));
            
            await Container.Received(1)
                .ReadItemAsync<User>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
            
            Container.Received(1)
                .GetItemQueryIterator<Group>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
            Mapper.ReceivedWithAnyArgs(groups.Length)
                .Map<Models.Group>(Arg.Is<Group>(g => groups.Any(grp => grp.Id == g.Id)));
        }
        
        [TestMethod]
        public async Task GetAsync_NotFound_Test()
        {
            //Arrange
            Container.ReadItemAsync<User>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            //Act
            var result = await _sut.GetAsync(ApplicationId, _id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();

            await Container.Received(1)
                .ReadItemAsync<User>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);

            Mapper.DidNotReceiveWithAnyArgs().Map<Models.User>(Arg.Any<User>());

            Container.DidNotReceiveWithAnyArgs()
                .GetItemQueryIterator<Group>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
            Mapper.DidNotReceiveWithAnyArgs().Map<Models.Group>(Arg.Any<Group>());
        }
        
        [TestMethod]
        public async Task PostAsync_Test()
        {
            //Arrange
            var user = Fixture.Create<Models.User>();
            var mappedUser = Fixture.Create<User>();

            Mapper.Map<User>(Arg.Any<Models.User>()).ReturnsForAnyArgs(mappedUser);

            Container.CreateItemAsync(Arg.Is(mappedUser), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, mappedUser));

            Mapper.Map<Models.User>(Arg.Any<User>()).Returns(user);
            
            //Act
            var result = await _sut.PostAsync(ApplicationId, user).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var userResult = ok.Value as Models.User;
            userResult.Should().NotBeNull();
            
            await Container.Received(1)
                .ReadItemAsync<Application>(Arg.Is(ApplicationId), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);

            await Container.Received(1)
                .CreateItemAsync(Arg.Is(mappedUser), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
            
            Mapper.Received(1).Map<User>(Arg.Any<Models.User>());
        }

        [TestMethod]
        public async Task PostAsync_ApplicationNotFound_Test()
        {
            //Arrange
            Container.ReadItemAsync<Application>(Arg.Is(ApplicationId), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            var user = Fixture.Create<Models.User>();
            
            //Act
            var result = await _sut.PostAsync(ApplicationId, user).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();
            
            await Container.Received(1)
                .ReadItemAsync<Application>(Arg.Is(ApplicationId), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);

            await Container.DidNotReceiveWithAnyArgs()
                .CreateItemAsync(Arg.Any<User>(), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
            
            Mapper.DidNotReceiveWithAnyArgs().Map<User>(Arg.Any<Models.User>());
        }
        
        [TestMethod]
        public async Task PutAsync_Test()
        {
            //Arrange
            var user = Fixture.Create<Models.User>();
            var document = Fixture.Create<User>();

            Container.ReadItemAsync<User>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, document));

            Mapper.Map(Arg.Any<Models.User>(), Arg.Any<User>()).Returns(document);
            
            //Act
            var result = await _sut.PutAsync(ApplicationId, _id, user).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var userResult = ok.Value as Models.User;
            userResult.Should().NotBeNull();

            await Container.Received(1)
                .ReadItemAsync<User>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task PutAsync_NotFound_Test()
        {
            //Arrange
            var user = Fixture.Create<Models.User>();

            Container.ReadItemAsync<User>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            //Act
            var result = await _sut.PutAsync(ApplicationId, _id, user).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();

            await Container.Received(1)
                .ReadItemAsync<User>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
       
        [TestMethod]
        public async Task DeleteAsync_Test()
        {
            //Arrange

            //Act
            var result = await _sut.DeleteAsync(ApplicationId, _id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();

            await Container.Received(1)
                .DeleteItemAsync<User>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
    }
}
