using AuthorizationManagement.Api.Extensions;
using AuthorizationManagement.Api.Models.Internal;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace AuthorizationManagement.Api.Controllers
{
    [ApiKey]
    [Route("api/applications/{applicationId}/[controller]")]
    [ApiController]
    public class UserGroupsController : ContainerControllerBase<UserGroup>
    {
        public UserGroupsController(Container container, IMapper mapper) 
            : base(container, mapper, DocumentType.UserGroup)
        {
        }

        [ProducesResponseType(typeof(Models.UserGroup), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromRoute]string applicationId, [FromBody]Models.UserGroup userGroupDto)
        {
            var query = new QueryDefinition(
                    $"SELECT * FROM c WHERE c.documentType = '{DocumentType}' AND c.applicationId = @applicationId AND c.userId = @userId AND c.groupId = @groupId")
                .WithParameter("@applicationId", applicationId)
                .WithParameter("@userId", userGroupDto.UserId)
                .WithParameter("@groupId", userGroupDto.GroupId);

            var userGroup = await Container.SingleOrDefaultAsync<UserGroup>(query).ConfigureAwait(false);
            if (userGroup != null) return Ok(Mapper.Map<Models.UserGroup>(userGroup));

            userGroup = Mapper.Map<UserGroup>(userGroupDto);
            userGroup.ApplicationId = applicationId;
            userGroup = await CreateAsync(userGroup).ConfigureAwait(false);

            return Ok(Mapper.Map<Models.UserGroup>(userGroup));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute]string applicationId, string id)
        {
            await Container.DeleteItemAsync<UserGroup>(id, new PartitionKey(applicationId)).ConfigureAwait(false);
            return Ok();
        }
    }
}
