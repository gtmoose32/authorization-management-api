using System.Collections.Generic;
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
        private UsersController _sut;
        
        [TestInitialize]
        public void Init()
        {
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
            const string id = "32";
            var user = Fixture.Build<Models.User>()
                .With(u => u.Id, id)
                .Without(u => u.Groups)
                .Create();
            
            var groups = Fixture.CreateMany<Group>().ToArray();
            var groupDtos = groups.Select(g => new Models.Group {Id = g.Id, Name = g.Name}).ToList();
            var document = Fixture.Build<User>().With(a => a.Id, id).Create();

            Container.ReadItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, document));
            
            var feedIterator = CreateFeedIteratorWithResponse(groups);
            Container.GetItemQueryIterator<Group>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            Mapper.Map<Models.User>(Arg.Any<User>()).ReturnsForAnyArgs(user);

            Mapper.Map<Models.Group>(Arg.Any<Group>()).Returns(info => groupDtos.Single(g=> g.Id == info.Arg<Group>().Id));
            
            //Act
            var result = await _sut.GetAsync(ApplicationId, id).ConfigureAwait(false);

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
                .ReadItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
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
            const string id = "32";
            
            Container.ReadItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            //Act
            var result = await _sut.GetAsync(ApplicationId, id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();

            await Container.Received(1)
                .ReadItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
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
            var groupResult = ok.Value as Models.User;
            groupResult.Should().NotBeNull();
            
            await Container.Received(1)
                .CreateItemAsync(Arg.Is(mappedUser), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
            
            Mapper.Received(1).Map<User>(Arg.Any<Models.User>());
        }
        
        [TestMethod]
        public async Task PutAsync_Test()
        {
            //Arrange
            const string id = "32";
            var user = Fixture.Create<Models.User>();
            var document = Fixture.Create<User>();

            Container.ReadItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, document));

            Mapper.Map(Arg.Any<Models.User>(), Arg.Any<User>()).Returns(document);
            
            //Act
            var result = await _sut.PutAsync(ApplicationId, id, user).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var groupResult = ok.Value as Models.User;
            groupResult.Should().NotBeNull();

            await Container.Received(1)
                .ReadItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task PutAsync_NotFound_Test()
        {
            //Arrange
            const string id = "32";
            var group = Fixture.Create<Models.User>();

            Container.ReadItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            //Act
            var result = await _sut.PutAsync(ApplicationId, id, group).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();

            await Container.Received(1)
                .ReadItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        //[TestMethod]
        //public async Task DeleteAsync_NoGroupsForUser_Test()
        //{
        //    //Arrange
        //    const string id = "32";
        //    var user = Fixture.Build<User>().With(g => g.Id, id).Create();
            
        //    var feedIterator = CreateFeedIteratorWithResponse(Enumerable.Empty<string>());
        //    Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
        //        .ReturnsForAnyArgs(feedIterator);
            
        //    Container.DeleteItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
        //        .Returns(CreateItemResponse(HttpStatusCode.OK, user));

        //    //Act
        //    var result = await _sut.DeleteAsync(ApplicationId, id).ConfigureAwait(false);

        //    //Assert
        //    result.Should().NotBeNull();
        //    result.Should().BeOfType<OkResult>();

        //    Container.Received(1)
        //        .GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
        //    await Container.Received(1)
        //        .DeleteItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
        //        .ConfigureAwait(false);
        //}
        
        [TestMethod]
        public async Task DeleteAsync_Test()
        {
            //Arrange
            const string id = "32";

            //Act
            var result = await _sut.DeleteAsync(ApplicationId, id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();

            await Container.Received(1)
                .DeleteItemAsync<User>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }

        //[TestMethod]
        //public async Task GetGroupsAsync_Test()
        //{
        //    //Arrange
        //    const string id = "32";
            
        //    var feedIterator = CreateFeedIteratorWithResponse(Fixture.CreateMany<string>());
        //    Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
        //        .ReturnsForAnyArgs(feedIterator);

        //    var groups = Fixture.CreateMany<Group>().ToArray();
        //    var userFeedIterator = CreateFeedIteratorWithResponse(groups);
        //    Container.GetItemQueryIterator<Group>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
        //        .ReturnsForAnyArgs(userFeedIterator);

        //    var mappedGroup = Fixture.Create<Models.Group>();
        //    Mapper.Map<Models.Group>(Arg.Any<Group>()).ReturnsForAnyArgs(mappedGroup);
                
        //    //Act
        //    var result = await _sut.GetGroupsAsync(ApplicationId, id).ConfigureAwait(false);

        //    //Assert
        //    result.Should().NotBeNull();
        //    result.Should().BeOfType<OkObjectResult>();

        //    var results = ((OkObjectResult) result).Value as Models.Group[];
        //    results.Should().NotBeNullOrEmpty();
        //    results.Length.Should().Be(groups.Length);
        //    results.All(g => ReferenceEquals(g, mappedGroup)).Should().BeTrue();

        //    Container.Received(1)
        //        .GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
        //    Container.Received(1)
        //        .GetItemQueryIterator<Group>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());

        //    Mapper.Received(groups.Length).Map<Models.Group>(Arg.Any<Group>());
        //}
        
        //[TestMethod]
        //public async Task GetUsersAsync_NoGroupsForUser_Test()
        //{
        //    //Arrange
        //    const string id = "32";

        //    var users = Enumerable.Empty<string>().ToArray();
        //    var feedIterator = CreateFeedIteratorWithResponse(users);
        //    Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
        //        .ReturnsForAnyArgs(feedIterator);

        //    //Act
        //    var result = await _sut.GetGroupsAsync(ApplicationId, id).ConfigureAwait(false);

        //    //Assert
        //    result.Should().NotBeNull();
        //    result.Should().BeOfType<OkObjectResult>();

        //    var results = ((OkObjectResult) result).Value as Models.User[];
        //    results.Should().BeNullOrEmpty();

        //    Container.Received(1)
        //        .GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
        //    Container.DidNotReceiveWithAnyArgs()
        //        .GetItemQueryIterator<User>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());

        //    Mapper.DidNotReceiveWithAnyArgs().Map<Models.User>(Arg.Any<User>());
        //}
    }
}
