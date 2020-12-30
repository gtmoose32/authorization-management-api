using AuthorizationManagement.Api.Extensions;
using AuthorizationManagement.Api.Models.Internal;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User = AuthorizationManagement.Api.Models.Internal.User;

namespace AuthorizationManagement.Api.Controllers
{
    [ApiKey]
    [Route("api/applications/{applicationId}/[controller]")]
    [ApiController]
    public class GroupsController : ContainerControllerBase<Group>
    {
        public GroupsController(Container container, IMapper mapper)
            : base(container, mapper, DocumentType.Group)
        {
        }

        [ProducesResponseType(typeof(IEnumerable<Models.Group>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromRoute] string applicationId)
        {
            var groups = await GetDocumentsAsync(applicationId).ConfigureAwait(false);
            return Ok(groups.Select(g => Mapper.Map<Models.Group>(g)).ToArray());
        }

        [ProducesResponseType(typeof(Models.Group), StatusCodes.Status200OK)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync([FromRoute] string applicationId, string id)
        {
            var group = await GetDocumentAsync(applicationId, id).ConfigureAwait(false);
            if (group == null) return NotFound();

            return Ok(Mapper.Map<Models.Group>(group));
        }

        [ProducesResponseType(typeof(IEnumerable<Models.User>), StatusCodes.Status200OK)]
        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetUsersAsync([FromRoute] string applicationId, string id)
        {
            var userIds = (await GetUserIdsFromGroupAsync(applicationId, id).ConfigureAwait(false)).ToArray();
            if (!userIds.Any()) return Ok(Enumerable.Empty<Models.User>());

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.documentType = 'User' AND c.applicationId = @applicationId AND c.id IN ({CreateInOperatorInput(userIds)})")
                .WithParameter("@applicationId", applicationId);

            var users = await Container.WhereAsync<User>(query).ConfigureAwait(false);

            return Ok(users.Select(u => Mapper.Map<Models.UserInfo>(u)).ToArray());
        }

        [ProducesResponseType(typeof(Models.Group), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromRoute] string applicationId, [FromBody] Models.Group groupDto)
        {
            if (!(await ApplicationExistsAsync(applicationId).ConfigureAwait(false)))
                return NotFound();

            var group = Mapper.Map<Group>(groupDto);
            group.ApplicationId = applicationId;

            await CreateDocumentAsync(group).ConfigureAwait(false);
            return Ok(groupDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync([FromRoute] string applicationId, string id, [FromBody] Models.Group groupDto)
        {
            var group = await GetDocumentAsync(applicationId, id).ConfigureAwait(false);
            if (group == null) return NotFound();

            group.Name = groupDto.Name;

            await UpdateDocumentAsync(group).ConfigureAwait(false);

            return Ok(groupDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string applicationId, string id)
        {
            var query = new QueryDefinition($"SELECT value c.id FROM c JOIN c.groups g WHERE c.documentType = '{DocumentType.User}' AND c.applicationId = @applicationId AND g.groupId = @groupId")
                .WithParameter("@applicationId", applicationId)
                .WithParameter("@groupId", id);

            var userIds = (await GetUserIdsFromGroupAsync(applicationId, id)).ToArray();
            if (userIds.Any())
                return Conflict(
                    new
                    {
                        Error = $"Cannot delete group with id '{id}' users are still assigned to this group.",
                        GroupAssignedUsers = userIds
                    });

            await Container.DeleteItemAsync<Group>(id, new PartitionKey(applicationId)).ConfigureAwait(false);

            return Ok();
        }

        private async Task<IEnumerable<string>> GetUserIdsFromGroupAsync(string applicationId, string groupId)
        {
            var query = new QueryDefinition($"SELECT value c.id FROM c JOIN c.groups g WHERE c.documentType = '{DocumentType.User}' AND c.applicationId = @applicationId AND g.groupId = @groupId")
                .WithParameter("@applicationId", applicationId)
                .WithParameter("@groupId", groupId);

            return await Container.WhereAsync<string>(query).ConfigureAwait(false);
        }
    }
}
