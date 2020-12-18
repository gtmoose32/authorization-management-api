using AuthorizationManagement.Api.Models.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationManagement.Api.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ContainerControllerBase<Application>
    {
        public ApplicationsController(Container container) 
            : base(container, DocumentType.Application)
        {
        }
        
        // GET: api/<ApplicationsController>
        [ProducesResponseType(typeof(IEnumerable<Models.Application>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.documentType = '{DocumentType}'");
            var options = new QueryRequestOptions { MaxItemCount = 1000 };

            var apps = await Container.WhereAsync<Application>(query, options).ConfigureAwait(false);
            return Ok(apps.Select(app => new { app.Id, app.Name, app.GroupCount, app.UserCount }));
        }
        
        // GET api/<ApplicationsController>/5
        [ProducesResponseType(typeof(Models.Application), StatusCodes.Status200OK)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(string id)
        {
            var app = await GetDocumentAsync(id, id).ConfigureAwait(false);
            if (app == null) return NotFound();

            return Ok(new { app.Id, app.Name, app.GroupCount, app.UserCount });
        }

        // POST api/<ApplicationsController>
        [ProducesResponseType(typeof(Models.Application), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] Models.Application appDto)
        {
            var app = new Application(appDto)
            {
                GroupCount = 0,
                UserCount = 0
            };

            app = await CreateAsync(app).ConfigureAwait(false);
            return Ok(new { app.Id, app.Name, app.GroupCount, app.UserCount });
        }
    }
}
