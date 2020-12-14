using AuthorizationManagement.Shared;
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
        protected DocumentType DocumentType { get; }

        protected ContainerControllerBase(Container container, DocumentType documentType)
        {
            Container = container;
            DocumentType = documentType;
        }

        protected virtual async Task<T> CreateAsync(T document) =>
            (await Container.CreateItemAsync(document, new PartitionKey(document.ApplicationId)).ConfigureAwait(false)).Resource;

        protected virtual async Task<IEnumerable<T>> GetDocumentsAsync(string partitionKey, int itemCount = 1000)
        {
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.applicationId = @applicationId AND c.documentType = '{DocumentType}'")
                .WithParameter("@applicationId", partitionKey);

            var options = new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey), MaxItemCount = itemCount };

            return await Container.WhereAsync<T>(query, options).ConfigureAwait(false);
        }

        protected virtual async Task<T> GetDocumentAsync(string applicationId, string id)
        {
            try
            {
                var response = await Container.ReadItemAsync<T>(id, new PartitionKey(applicationId))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

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

        protected Task IncrementUserCountAsync(string applicationId) => IncrementCountAsync(applicationId, DocumentType.User);

        protected Task IncrementGroupCountAsync(string applicationId) => IncrementCountAsync(applicationId, DocumentType.Group);

        private async Task IncrementCountAsync(string applicationId, DocumentType documentType)
        {
            var response = await Container.ReadItemAsync<Application>(applicationId, new PartitionKey(applicationId))
                .ConfigureAwait(false);

            var app = response.Resource;
            switch (documentType)
            {
                case DocumentType.User:
                    app.UserCount++;
                    break;
                case DocumentType.Group:
                    app.GroupCount++;
                    break;
                case DocumentType.Unknown:
                case DocumentType.Application:
                case DocumentType.UserGroup:
                    return;
            }

            await Container.ReplaceItemAsync(app, applicationId, new PartitionKey(applicationId), new ItemRequestOptions { IfMatchEtag = app.ETag })
                .ConfigureAwait(false);
        }
    }
}
