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
    public class UsersController : ContainerControllerBase<User>
    {
        public UsersController(Container container)
            : base(container, DocumentType.User)
        {
        }

        // GET: api/<UsersController>
        [ProducesResponseType(typeof(IEnumerable<Models.User>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromRoute] string applicationId)
        {
            var users = await GetDocumentsAsync(applicationId).ConfigureAwait(false);
            return Ok(users.Select(u => new { u.Email, u.Id, u.FirstName, u.LastName, u.Enabled }));
        }

        // GET api/<UsersController>/5
        [ProducesResponseType(typeof(Models.User), StatusCodes.Status200OK)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync([FromRoute] string applicationId, string id)
        {
            var user = await GetDocumentAsync(applicationId, id).ConfigureAwait(false);
            if (user == null) return NotFound();

            return Ok(new { user.FirstName, user.LastName, user.Email, user.Id, user.Enabled });
        }

        [ProducesResponseType(typeof(IEnumerable<Models.Group>), StatusCodes.Status200OK)]
        [HttpGet("{id}/groups")]
        public async Task<IActionResult> GetGroupsAsync([FromRoute] string applicationId, string id)
        {
            var groupIds = (await GetGroupIdsFromUserAsync(applicationId, id).ConfigureAwait(false)).ToArray();
            if (!groupIds.Any()) return Ok(Enumerable.Empty<Models.Group>());

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.documentType = 'Group' AND c.applicationId = @applicationId AND c.id IN ({CreateInOperatorInput(groupIds)})")
                .WithParameter("@applicationId", applicationId);

            var groups = await Container.WhereAsync<Group>(query).ConfigureAwait(false);

            return Ok(groups.Select(g => new { g.Id, g.Name }));
        }

        // POST api/<UsersController>
        [ProducesResponseType(typeof(Models.User), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromRoute] string applicationId, [FromBody] Models.User userDto)
        {
            var user = new User(userDto) { ApplicationId = applicationId };

            await CreateAsync(user).ConfigureAwait(false);
            await IncrementUserCountAsync(applicationId).ConfigureAwait(false);

            return Ok(userDto);
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync([FromRoute] string applicationId, string id, [FromBody] Models.User userDto)
        {
            var user = await GetDocumentAsync(applicationId, id).ConfigureAwait(false);
            if (user == null) return NotFound();

            user.Email = userDto.Email;
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Enabled = userDto.Enabled;

            await Container.ReplaceItemAsync(user, id, new PartitionKey(applicationId), new ItemRequestOptions { IfMatchEtag = user.ETag })
                .ConfigureAwait(false);

            return Ok();
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string applicationId, string id)
        {
            var query = new QueryDefinition($"SELECT value c.id FROM c WHERE c.documentType = '{DocumentType.UserGroup}' AND c.applicationId = @applicationId AND c.userId = @userId")
                .WithParameter("@applicationId", applicationId)
                .WithParameter("@userId", id);


            var userGroupIds = (await Container.WhereAsync<string>(query).ConfigureAwait(false)).ToArray();
            if (userGroupIds.Any())
                return BadRequest(
                    new
                    {
                        Error = $"Cannot delete Group with id '{id}' due to UserGroup(s) referencing this group.",
                        ReferencingGroups = userGroupIds
                    });

            var batch = Container.CreateTransactionalBatch(new PartitionKey(applicationId))
                .DeleteItem(id);

            foreach (var userGroupId in userGroupIds)
            {
                batch.DeleteItem(userGroupId);
            }

            await batch.ExecuteAsync().ConfigureAwait(false);

            return Ok();
        }

        private async Task<IEnumerable<string>> GetGroupIdsFromUserAsync(string applicationId, string userId)
        {
            var query = new QueryDefinition("SELECT VALUE c.groupId FROM c WHERE c.documentType = 'UserGroup' AND c.applicationId = @applicationId AND c.userId = @userId")
                .WithParameter("@applicationId", applicationId)
                .WithParameter("@userId", userId);

            return await Container.WhereAsync<string>(query).ConfigureAwait(false);
        }
    }
}
