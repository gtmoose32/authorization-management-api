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
    public class UsersController : ContainerControllerBase<User>
    {
        public UsersController(Container container, IMapper mapper)
            : base(container, mapper, DocumentType.User)
        {
        }

        [ProducesResponseType(typeof(IEnumerable<Models.UserInfo>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromRoute] string applicationId)
        {
            var users = await GetDocumentsAsync(applicationId).ConfigureAwait(false);
            return Ok(users.Select(u => Mapper.Map<Models.UserInfo>(u)).ToArray());
        }

        [ProducesResponseType(typeof(Models.User), StatusCodes.Status200OK)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync([FromRoute] string applicationId, string id)
        {
            var user = await GetDocumentAsync(applicationId, id).ConfigureAwait(false);
            if (user == null) return NotFound();

            var response = Mapper.Map<Models.User>(user);
            
            var query = new QueryDefinition(
                $"SELECT * FROM c WHERE c.documentType = '{DocumentType.Group}' AND c.id IN ({CreateInOperatorInput(user.Groups.ToArray())})");

            var groups = await Container.WhereAsync<Group>(query).ConfigureAwait(false);
            response.Groups = groups.Select(g => Mapper.Map<Models.Group>(g)).ToList();
            
            return Ok(response);
        }

        [ProducesResponseType(typeof(Models.User), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromRoute] string applicationId, [FromBody] Models.User userDto)
        {
            if (!(await ApplicationExistsAsync(applicationId).ConfigureAwait(false)))
                return NotFound();

            var user = Mapper.Map<User>(userDto);
            user.ApplicationId = applicationId;
            
            await CreateDocumentAsync(user).ConfigureAwait(false);

            return Ok(userDto);
        }

        [ProducesResponseType(typeof(Models.User), StatusCodes.Status200OK)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync([FromRoute] string applicationId, string id, [FromBody] Models.User userDto)
        {
            var user = await GetDocumentAsync(applicationId, id).ConfigureAwait(false);
            if (user == null) return NotFound();

            user = Mapper.Map(userDto, user);

            await UpdateDocumentAsync(user).ConfigureAwait(false);

            return Ok(userDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string applicationId, string id)
        {
            await Container.DeleteItemAsync<User>(id, new PartitionKey(applicationId)).ConfigureAwait(false);
            return Ok();
        }
    }
}
