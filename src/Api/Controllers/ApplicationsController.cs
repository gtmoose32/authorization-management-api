using AuthorizationManagement.Api.Extensions;
using AuthorizationManagement.Api.Models.Internal;
using AutoMapper;
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
        public ApplicationsController(Container container, IMapper mapper) 
            : base(container, mapper, DocumentType.Application)
        {
        }
        
        [ProducesResponseType(typeof(IEnumerable<Models.Application>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.documentType = '{DocumentType}'");
            var options = new QueryRequestOptions { MaxItemCount = 1000 };
            
            var apps = await Container.WhereAsync<Application>(query, options).ConfigureAwait(false);
            return Ok(apps.Select(a => Mapper.Map<Models.Application>(a)).ToArray());
        }
        
        [ProducesResponseType(typeof(Models.Application), StatusCodes.Status200OK)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(string id)
        {
            var app = await GetDocumentAsync(id, id).ConfigureAwait(false);
            if (app == null) return NotFound();

            return Ok(Mapper.Map<Models.Application>(app));
        }

        [ProducesResponseType(typeof(Models.Application), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] Models.Application appDto)
        {
            var app = await CreateAsync(Mapper.Map<Application>(appDto)).ConfigureAwait(false);
            return Ok(Mapper.Map<Models.Application>(app));
        }
    }
}
