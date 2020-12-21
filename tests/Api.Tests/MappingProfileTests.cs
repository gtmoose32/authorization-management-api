using AuthorizationManagement.Api.Models.Internal;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace AuthorizationManagement.Api.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MappingProfileTests
    {
        private readonly IFixture _fixture;
        private readonly IMapper _sut;
        
        public MappingProfileTests()
        {
            _fixture = new Fixture();
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
            config.AssertConfigurationIsValid();

            _sut = config.CreateMapper();
        }
        
        [TestMethod]
        public void Application_MapToDocument_Test()
        {
            //Arrange
            var app = _fixture.Create<Models.Application>();

            //Act
            var result = _sut.Map<Application>(app);
            
            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(app.Id);
            result.Name.Should().Be(app.Name);
        }

        [TestMethod]
        public void Application_MapToDto_Test()
        {
            //Arrange
            var app = _fixture.Create<Application>();

            //Act
            var result = _sut.Map<Models.Application>(app);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(app.Id);
            result.Name.Should().Be(app.Name);
        }

        [TestMethod]
        public void Group_MapToDocument_Test()
        {
            //Arrange
            var grp = _fixture.Create<Models.Group>();

            //Act
            var result = _sut.Map<Group>(grp);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(grp.Id);
            result.Name.Should().Be(grp.Name);
        }

        [TestMethod]
        public void Group_MapToDto_Test()
        {
            //Arrange
            var grp = _fixture.Create<Group>();

            //Act
            var result = _sut.Map<Models.Group>(grp);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(grp.Id);
            result.Name.Should().Be(grp.Name);
        }

        [TestMethod]
        public void User_MapToDocument_Test()
        {
            //Arrange
            var user = _fixture.Create<Models.User>();

            //Act
            var result = _sut.Map<User>(user);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
            result.Enabled.Should().Be(user.Enabled);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
        }

        [TestMethod]
        public void User_MapToDto_Test()
        {
            //Arrange
            var user = _fixture.Create<User>();

            //Act
            var result = _sut.Map<Models.User>(user);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
            result.Enabled.Should().Be(user.Enabled);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
        }

        [TestMethod]
        public void UserGroup_MapToDocument_Test()
        {
            //Arrange
            var ug = _fixture.Create<Models.UserGroup>();

            //Act
            var result = _sut.Map<UserGroup>(ug);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(ug.Id);
            result.GroupId.Should().Be(ug.GroupId);
            result.UserId.Should().Be(ug.UserId);
        }

        [TestMethod]
        public void UserGroup_MapToDto_Test()
        {
            //Arrange
            var ug = _fixture.Create<UserGroup>();

            //Act
            var result = _sut.Map<Models.UserGroup>(ug);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(ug.Id);
            result.GroupId.Should().Be(ug.GroupId);
            result.UserId.Should().Be(ug.UserId);
        }
    }
}