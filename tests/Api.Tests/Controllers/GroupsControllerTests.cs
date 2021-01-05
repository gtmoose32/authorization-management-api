﻿using AuthorizationManagement.Api.Controllers;
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
    public class GroupsControllerTests : ControllerTestBase
    {
        private string _id;
        private GroupsController _sut;
        
        [TestInitialize]
        public void Init()
        {
            _id = Fixture.Create<string>();
            _sut = new GroupsController(Container, Mapper);
        }
        
        [TestMethod]
        public async Task GetAllAsync_Test()
        {
            //Arrange
            var groups = Fixture.CreateMany<Group>().ToArray();
            var mappedGroup = Fixture.Create<Models.Group>();
            
            var feedIterator = CreateFeedIteratorWithResponse(groups);
            Container.GetItemQueryIterator<Group>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            Mapper.Map<Models.Group>(Arg.Any<Group>()).ReturnsForAnyArgs(mappedGroup);
            
            //Act
            var result = await _sut.GetAllAsync(ApplicationId).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult) result;
            ok.Value.Should().NotBeNull();
            var results = (Models.Group[]) ok.Value;
            results.Should().NotBeNullOrEmpty();
            results.Length.Should().Be(groups.Length);
            results.All(a => ReferenceEquals(a, mappedGroup)).Should().Be(true);

            Mapper.Received(groups.Length).Map<Models.Group>(Arg.Any<Group>());
            Container.Received(1)
                .GetItemQueryIterator<Group>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
        }
        
        [TestMethod]
        public async Task GetAsync_Test()
        {
            //Arrange
            var group = Fixture.Build<Group>().With(a => a.Id, _id).Create();
            var mappedGroup = new Models.Group { Id = _id, Name = group.Name };

            Container.ReadItemAsync<Group>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, group));

            Mapper.Map<Models.Group>(Arg.Any<Group>()).ReturnsForAnyArgs(mappedGroup);
            
            //Act
            var result = await _sut.GetAsync(ApplicationId, _id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var groupResult = ok.Value as Models.Group;
            groupResult.Should().NotBeNull();

            Mapper.Received(1).Map<Models.Group>(Arg.Any<Group>());
            await Container.Received(1)
                .ReadItemAsync<Group>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task GetAsync_NotFound_Test()
        {
            //Arrange
            Container.ReadItemAsync<Group>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            //Act
            var result = await _sut.GetAsync(ApplicationId, _id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();

            await Container.Received(1)
                .ReadItemAsync<Group>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);

            Mapper.DidNotReceiveWithAnyArgs().Map<Models.Group>(Arg.Any<Group>());
        }
        
        [TestMethod]
        public async Task PostAsync_Test()
        {
            //Arrange
            var group = Fixture.Create<Models.Group>();
            var mappedGroup = new Group { Id = group.Id, Name = group.Name };

            Mapper.Map<Group>(Arg.Any<Models.Group>()).ReturnsForAnyArgs(mappedGroup);

            Container.CreateItemAsync(Arg.Is(mappedGroup), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, mappedGroup));

            Mapper.Map<Models.Group>(Arg.Any<Group>()).Returns(group);
            
            //Act
            var result = await _sut.PostAsync(ApplicationId, group).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var groupResult = ok.Value as Models.Group;
            groupResult.Should().NotBeNull();

            await Container.Received(1)
                .ReadItemAsync<Application>(Arg.Is(ApplicationId), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);

            await Container.Received(1)
                .CreateItemAsync(Arg.Is(mappedGroup), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
            
            Mapper.Received(1).Map<Group>(Arg.Any<Models.Group>());
        }

        [TestMethod]
        public async Task PostAsync_ApplicationNotFound_Test()
        {
            //Arrange
            Container.ReadItemAsync<Application>(Arg.Is(ApplicationId), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            var group = Fixture.Create<Models.Group>();
            
            //Act
            var result = await _sut.PostAsync(ApplicationId, group).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();
            
            await Container.Received(1)
                .ReadItemAsync<Application>(Arg.Is(ApplicationId), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);

            await Container.DidNotReceiveWithAnyArgs()
                .CreateItemAsync(Arg.Any<Group>(), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
            
            Mapper.DidNotReceiveWithAnyArgs().Map<Group>(Arg.Any<Models.Group>());
        }
        
        [TestMethod]
        public async Task PutAsync_Test()
        {
            //Arrange
            var group = Fixture.Create<Models.Group>();
            var document = new Group { Id = _id, ApplicationId = ApplicationId, Name = group.Name };

            Container.ReadItemAsync<Group>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, document));

            //Act
            var result = await _sut.PutAsync(ApplicationId, _id, group).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var groupResult = ok.Value as Models.Group;
            groupResult.Should().NotBeNull();

            await Container.Received(1)
                .ReadItemAsync<Group>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task PutAsync_NotFound_Test()
        {
            //Arrange
            var group = Fixture.Create<Models.Group>();

            Container.ReadItemAsync<Group>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            //Act
            var result = await _sut.PutAsync(ApplicationId, _id, group).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();

            await Container.Received(1)
                .ReadItemAsync<Group>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task DeleteAsync_Test()
        {
            //Arrange
            var feedIterator = CreateFeedIteratorWithResponse(Enumerable.Empty<string>());
            Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);
            
            //Act
            var result = await _sut.DeleteAsync(ApplicationId, _id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();

            Container.Received(1)
                .GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
            await Container.Received(1)
                .DeleteItemAsync<Group>(Arg.Is(_id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task DeleteAsync_Conflict_Test()
        {
            //Arrange
            var app = Fixture.Create<Application>();
            Container.ReadItemAsync<Application>(Arg.Is(ApplicationId), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, app));

            var feedIterator = CreateFeedIteratorWithResponse(Fixture.CreateMany<string>());
            Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            //Act
            var result = await _sut.DeleteAsync(ApplicationId, _id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ConflictObjectResult>();

            Container.Received(1)
                .GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
            await Container.DidNotReceiveWithAnyArgs()
                .DeleteItemAsync<Group>(Arg.Any<string>(), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task GetUsersAsync_Test()
        {
            //Arrange
            var feedIterator = CreateFeedIteratorWithResponse(Fixture.CreateMany<string>());
            Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            var users = Fixture.CreateMany<User>().ToArray();
            var userFeedIterator = CreateFeedIteratorWithResponse(users);
            Container.GetItemQueryIterator<User>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(userFeedIterator);

            var mappedUser = Fixture.Create<Models.UserInfo>();
            Mapper.Map<Models.UserInfo>(Arg.Any<User>()).ReturnsForAnyArgs(mappedUser);

            //Act
            var result = await _sut.GetUsersAsync(ApplicationId, _id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();

            var results = ((OkObjectResult)result).Value as Models.UserInfo[];
            results.Should().NotBeNullOrEmpty();
            results.Length.Should().Be(users.Length);
            results.All(u => ReferenceEquals(u, mappedUser)).Should().BeTrue();

            Container.Received(1)
                .GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());

            Container.Received(1)
                .GetItemQueryIterator<User>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());

            Mapper.Received(users.Length).Map<Models.UserInfo>(Arg.Any<User>());
        }

        [TestMethod]
        public async Task GetUsersAsync_NoUsersInGroup_Test()
        {
            //Arrange
            var users = Enumerable.Empty<string>().ToArray();
            var feedIterator = CreateFeedIteratorWithResponse(users);
            Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            //Act
            var result = await _sut.GetUsersAsync(ApplicationId, _id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();

            var results = ((OkObjectResult)result).Value as Models.User[];
            results.Should().BeNullOrEmpty();

            Container.Received(1)
                .GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());

            Container.DidNotReceiveWithAnyArgs()
                .GetItemQueryIterator<User>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());

            Mapper.DidNotReceiveWithAnyArgs().Map<Models.User>(Arg.Any<User>());
        }
    }
}
