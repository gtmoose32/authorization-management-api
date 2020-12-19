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
    public class GroupsControllerTests : ControllerTestBase
    {
        private const string ApplicationId = "2321";
        
        private GroupsController _sut;
        
        [TestInitialize]
        public void Init()
        {
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
            const string id = "32";
            var group = Fixture.Build<Group>().With(a => a.Id, id).Create();
            var mappedGroup = new Models.Group { Id = id, Name = group.Name };

            Container.ReadItemAsync<Group>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, group));

            Mapper.Map<Models.Group>(Arg.Any<Group>()).ReturnsForAnyArgs(mappedGroup);
            
            //Act
            var result = await _sut.GetAsync(ApplicationId, id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var groupResult = ok.Value as Models.Group;
            groupResult.Should().NotBeNull();

            Mapper.Received(1).Map<Models.Group>(Arg.Any<Group>());
            await Container.Received(1)
                .ReadItemAsync<Group>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task GetAsync_NotFound_Test()
        {
            //Arrange
            const string id = "32";
            
            Container.ReadItemAsync<Group>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            //Act
            var result = await _sut.GetAsync(ApplicationId, id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();

            await Container.Received(1)
                .ReadItemAsync<Group>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);

            Mapper.DidNotReceiveWithAnyArgs().Map<Models.Group>(Arg.Any<Group>());
        }
        
        [TestMethod]
        public async Task PostAsync_Test()
        {
            //Arrange
            var app = Fixture.Build<Application>().With(a => a.Id, ApplicationId).Create();
            var group = Fixture.Create<Models.Group>();
            var mappedGroup = new Group { Id = group.Id, Name = group.Name };

            Mapper.Map<Group>(Arg.Any<Models.Group>()).ReturnsForAnyArgs(mappedGroup);

            Container.CreateItemAsync(Arg.Is(mappedGroup), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, mappedGroup));

            Container.ReadItemAsync<Application>(Arg.Is(ApplicationId), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, app));

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
                .CreateItemAsync(Arg.Is(mappedGroup), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
            
            Mapper.Received(1).Map<Group>(Arg.Any<Models.Group>());
        }
        
        [TestMethod]
        public async Task PutAsync_Test()
        {
            //Arrange
            const string id = "32";
            var group = Fixture.Create<Models.Group>();
            var document = new Group { Id = id, ApplicationId = ApplicationId, Name = group.Name };

            Container.ReadItemAsync<Group>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, document));

            //Act
            var result = await _sut.PutAsync(ApplicationId, id, group).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var groupResult = ok.Value as Models.Group;
            groupResult.Should().NotBeNull();

            await Container.Received(1)
                .ReadItemAsync<Group>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task PutAsync_NotFound_Test()
        {
            //Arrange
            const string id = "32";
            var group = Fixture.Create<Models.Group>();

            Container.ReadItemAsync<Group>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Throws(new CosmosException("Test document not found", HttpStatusCode.NotFound, 404, "test", 1.0));

            //Act
            var result = await _sut.PutAsync(ApplicationId, id, group).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();

            await Container.Received(1)
                .ReadItemAsync<Group>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task DeleteAsync_Test()
        {
            //Arrange
            const string id = "32";
            var group = Fixture.Build<Group>().With(g => g.Id, id).Create();
            
            var feedIterator = CreateFeedIteratorWithResponse(Enumerable.Empty<string>());
            Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);
            
            Container.DeleteItemAsync<Group>(Arg.Is(id), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, group));

            //Act
            var result = await _sut.DeleteAsync(ApplicationId, id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();

            Container.Received(1)
                .GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
            await Container.Received(1)
                .DeleteItemAsync<Group>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        [TestMethod]
        public async Task DeleteAsync_Conflict_Test()
        {
            //Arrange
            const string id = "32";
            
            var feedIterator = CreateFeedIteratorWithResponse(Fixture.CreateMany<string>());
            Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            //Act
            var result = await _sut.DeleteAsync(ApplicationId, id).ConfigureAwait(false);

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
            const string id = "32";
            
            var feedIterator = CreateFeedIteratorWithResponse(Fixture.CreateMany<string>());
            Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            var users = Fixture.CreateMany<User>().ToArray();
            var userFeedIterator = CreateFeedIteratorWithResponse(users);
            Container.GetItemQueryIterator<User>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(userFeedIterator);

            var mappedUser = Fixture.Create<Models.User>();
            Mapper.Map<Models.User>(Arg.Any<User>()).ReturnsForAnyArgs(mappedUser);
                
            //Act
            var result = await _sut.GetUsersAsync(ApplicationId, id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();

            var results = ((OkObjectResult) result).Value as Models.User[];
            results.Should().NotBeNullOrEmpty();
            results.Length.Should().Be(users.Length);
            results.All(u => ReferenceEquals(u, mappedUser)).Should().BeTrue();

            Container.Received(1)
                .GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
            Container.Received(1)
                .GetItemQueryIterator<User>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());

            Mapper.Received(users.Length).Map<Models.User>(Arg.Any<User>());
        }
        
        [TestMethod]
        public async Task GetUsersAsync_NoUsersInGroup_Test()
        {
            //Arrange
            const string id = "32";

            var users = Enumerable.Empty<string>().ToArray();
            var feedIterator = CreateFeedIteratorWithResponse(users);
            Container.GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .ReturnsForAnyArgs(feedIterator);

            //Act
            var result = await _sut.GetUsersAsync(ApplicationId, id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();

            var results = ((OkObjectResult) result).Value as Models.User[];
            results.Should().BeNullOrEmpty();

            Container.Received(1)
                .GetItemQueryIterator<string>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
            Container.DidNotReceiveWithAnyArgs()
                .GetItemQueryIterator<User>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());

            Mapper.DidNotReceiveWithAnyArgs().Map<Models.User>(Arg.Any<User>());
        }
        
        
        //public async Task<IActionResult> GetUsersAsync([FromRoute] string applicationId, string id)
        //{
        //    var userIds = (await GetUserIdsFromGroupAsync(applicationId, id).ConfigureAwait(false)).ToArray();
        //    if (!userIds.Any()) return Ok(Enumerable.Empty<Models.User>());

        //    var query = new QueryDefinition($"SELECT * FROM c WHERE c.documentType = 'User' AND c.applicationId = @applicationId AND c.id IN ({CreateInOperatorInput(userIds)})")
        //        .WithParameter("@applicationId", applicationId);

        //    var users = await Container.WhereAsync<User>(query).ConfigureAwait(false);

        //    return Ok(users.Select(u => Mapper.Map<Models.User>(u)).ToArray());
        //}
        
        //private async Task<IEnumerable<string>> GetUserIdsFromGroupAsync(string applicationId, string groupId)
        //{
        //    var query = new QueryDefinition("SELECT VALUE c.userId FROM c WHERE c.documentType = 'UserGroup' AND c.applicationId = @applicationId AND c.groupId = @groupId")
        //        .WithParameter("@applicationId", applicationId)
        //        .WithParameter("@groupId", groupId);

        //    return await Container.WhereAsync<string>(query).ConfigureAwait(false);
        //}
    }
}
