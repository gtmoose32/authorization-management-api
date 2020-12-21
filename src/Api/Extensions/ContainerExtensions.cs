using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationManagement.Api.Extensions
{
    public static class ContainerExtensions
    {
        public static async Task<T> SingleOrDefaultAsync<T>(this Container container, QueryDefinition query)
        {
            var results = await container.WhereAsync<T>(query, new QueryRequestOptions { MaxItemCount = 1 })
                .ConfigureAwait(false);

            return results.SingleOrDefault();
        }

        public static async Task<IEnumerable<T>> WhereAsync<T>(this Container container, QueryDefinition query, QueryRequestOptions options = null)
        {
            var results = new List<T>();
            using var iterator = container.GetItemQueryIterator<T>(query, requestOptions: options);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync().ConfigureAwait(false);
                results.AddRange(response.Resource);
            }

            return results;
        }
    }
}