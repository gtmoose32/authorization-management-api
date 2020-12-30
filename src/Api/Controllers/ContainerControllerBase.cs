using AuthorizationManagement.Api.Extensions;
using AuthorizationManagement.Api.Models.Internal;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationManagement.Api.Controllers
{
    public abstract class ContainerControllerBase<T> : ControllerBase
        where T : class, IDocument
    {
        protected Container Container { get; }
        
        protected IMapper Mapper { get; }
        
        protected DocumentType DocumentType { get; }

        protected ContainerControllerBase(Container container, IMapper mapper, DocumentType documentType)
        {
            Container = container;
            Mapper = mapper;
            DocumentType = documentType;
        }

        protected async Task<bool> ApplicationExistsAsync(string id)
        {
            try
            {
                await Container.ReadItemAsync<Application>(id, new PartitionKey(id)).ConfigureAwait(false);
                return true;
            }
            catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        protected async Task<T> CreateDocumentAsync(T document) =>
            (await Container.CreateItemAsync(document, new PartitionKey(document.ApplicationId)).ConfigureAwait(false)).Resource;

        protected async Task<IEnumerable<T>> GetDocumentsAsync(string partitionKey, int itemCount = 1000)
        {
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.applicationId = @applicationId AND c.documentType = '{DocumentType}'")
                .WithParameter("@applicationId", partitionKey);

            var options = new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey), MaxItemCount = itemCount };

            return await Container.WhereAsync<T>(query, options).ConfigureAwait(false);
        }

        protected async Task<T> GetDocumentAsync(string applicationId, string id)
        {
            try
            {
                var response = await Container.ReadItemAsync<T>(id, new PartitionKey(applicationId))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        protected Task UpdateDocumentAsync(T document) =>
            Container.ReplaceItemAsync(document, document.Id, new PartitionKey(document.ApplicationId), new ItemRequestOptions {IfMatchEtag = document.ETag});

        protected string CreateInOperatorInput(string[] values)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < values.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append($"'{values[i]}'");
                    continue;
                }

                sb.Append($", '{values[i]}'");
            }

            return sb.ToString();
        }
    }
}
