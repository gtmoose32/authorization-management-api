using AuthorizationManagement.Api.Controllers;
using AuthorizationManagement.Api.Models.Internal;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
// ReSharper disable PossibleNullReferenceException


namespace AuthorizationManagement.Api.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class UserGroupsControllerTests : ControllerTestBase
    {
        private UserGroupsController _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new UserGroupsController(Container, Mapper);
        }
        
        [TestMethod]
        public async Task PostAsync_Test()
        {
            //Arrange
            var userGroup = Fixture.Create<Models.UserGroup>();
            var document = Fixture.Create<UserGroup>();

            Mapper.Map<UserGroup>(Arg.Any<Models.UserGroup>()).ReturnsForAnyArgs(document);

            var feedIterator = CreateFeedIteratorWithResponse(Enumerable.Empty<UserGroup>());
            Container.GetItemQueryIterator<UserGroup>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .Returns(feedIterator);
                
            Container.CreateItemAsync(Arg.Is(document), Arg.Any<PartitionKey>())
                .Returns(CreateItemResponse(HttpStatusCode.OK, document));

            Mapper.Map<Models.UserGroup>(Arg.Any<UserGroup>()).Returns(userGroup);

            //Act
            var result = await _sut.PostAsync(ApplicationId, userGroup).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var ugResult = ok.Value as Models.UserGroup;
            ugResult.Should().NotBeNull();

            Mapper.Received(1).Map<UserGroup>(Arg.Any<Models.UserGroup>());

            Container.Received(1)
                .GetItemQueryIterator<UserGroup>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());
            
            await Container.Received(1)
                .CreateItemAsync(Arg.Is(document), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);

            Mapper.Received(1).Map<Models.UserGroup>(Arg.Any<UserGroup>());
        }

        [TestMethod]
        public async Task PostAsync_UserGroupAlreadyExists_Test()
        {
            //Arrange
            var userGroup = Fixture.Create<Models.UserGroup>();
            var document = Fixture.Create<UserGroup>();

            var feedIterator = CreateFeedIteratorWithResponse(new[] {document});
            Container.GetItemQueryIterator<UserGroup>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>())
                .Returns(feedIterator);

            Mapper.Map<Models.UserGroup>(Arg.Any<UserGroup>()).Returns(userGroup);

            //Act
            var result = await _sut.PostAsync(ApplicationId, userGroup).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var ugResult = ok.Value as Models.UserGroup;
            ugResult.Should().NotBeNull();
            
            Container.Received(1)
                .GetItemQueryIterator<UserGroup>(Arg.Any<QueryDefinition>(), requestOptions: Arg.Any<QueryRequestOptions>());

            Mapper.Received(1).Map<Models.UserGroup>(Arg.Any<UserGroup>());
            
            Mapper.DidNotReceiveWithAnyArgs().Map<UserGroup>(Arg.Any<Models.UserGroup>());

            await Container.DidNotReceiveWithAnyArgs()
                .CreateItemAsync(Arg.Is(document), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeleteAsync_Test()
        {
            //Arrange
            var id = Fixture.Create<string>();

            //Act
            var result = await _sut.DeleteAsync(ApplicationId, id).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();

            await Container.Received(1)
                .DeleteItemAsync<UserGroup>(Arg.Is(id), Arg.Any<PartitionKey>())
                .ConfigureAwait(false);
        }
        
        //// DELETE api/<UsersController>/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteAsync([FromRoute]string applicationId, string id)
        //{
        //    await Container.DeleteItemAsync<UserGroup>(id, new PartitionKey(applicationId)).ConfigureAwait(false);
        //    return Ok();
        //}
    }
}
