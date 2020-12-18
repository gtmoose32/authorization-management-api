using AuthorizationManagement.Api.Models.Internal;
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
        public GroupsController(Container container)
            : base(container, DocumentType.Group)
        {
        }

        // GET: api/<UsersController>
        [ProducesResponseType(typeof(IEnumerable<Models.Group>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromRoute] string applicationId)
        {
            var groups = await GetDocumentsAsync(applicationId).ConfigureAwait(false);
            return Ok(groups.Select(g => new { g.Id, g.Name }));
        }

        // GET api/<UsersController>/5
        [ProducesResponseType(typeof(Models.Group), StatusCodes.Status200OK)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync([FromRoute] string applicationId, string id)
        {
            var group = await GetDocumentAsync(applicationId, id).ConfigureAwait(false);
            if (group == null) return NotFound();

            return Ok(new { group.Id, group.Name });
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

            return Ok(users.Select(u => new { u.Id, u.Email, u.Enabled, u.FirstName, u.LastName }));
        }

        // POST api/<UsersController>
        [ProducesResponseType(typeof(Models.Group), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromRoute] string applicationId, [FromBody] Models.Group groupDto)
        {
            var group = new Group(groupDto) { ApplicationId = applicationId };

            await CreateAsync(group).ConfigureAwait(false);
            await IncrementGroupCountAsync(applicationId).ConfigureAwait(false);

            return Ok(groupDto);
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync([FromRoute] string applicationId, string id, [FromBody] Models.Group groupDto)
        {
            var group = await GetDocumentAsync(applicationId, id).ConfigureAwait(false);
            if (group == null) return NotFound();

            group.Name = groupDto.Name;

            await Container.ReplaceItemAsync(group, id, new PartitionKey(applicationId), new ItemRequestOptions { IfMatchEtag = group.ETag })
                .ConfigureAwait(false);

            return Ok();
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string applicationId, string id)
        {
            var query = new QueryDefinition($"SELECT value c.id FROM c WHERE c.documentType = '{DocumentType.UserGroup}' AND c.applicationId = @applicationId AND c.groupId = @groupId")
                .WithParameter("@applicationId", applicationId)
                .WithParameter("@groupId", id);

            var userGroupIds = (await Container.WhereAsync<string>(query).ConfigureAwait(false)).ToArray();
            if (userGroupIds.Any())
                return BadRequest(
                    new
                    {
                        Error = $"Cannot delete Group with id '{id}' due to UserGroup(s) referencing this group.",
                        ReferencingGroups = userGroupIds
                    });

            await Container.DeleteItemAsync<Group>(id, new PartitionKey(applicationId)).ConfigureAwait(false);

            return Ok();
        }

        private async Task<IEnumerable<string>> GetUserIdsFromGroupAsync(string applicationId, string groupId)
        {
            var query = new QueryDefinition("SELECT VALUE c.userId FROM c WHERE c.documentType = 'UserGroup' AND c.applicationId = @applicationId AND c.groupId = @groupId")
                .WithParameter("@applicationId", applicationId)
                .WithParameter("@groupId", groupId);

            return await Container.WhereAsync<string>(query).ConfigureAwait(false);
        }
    }
}
