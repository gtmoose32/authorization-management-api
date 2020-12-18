using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace AuthorizationManagement.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class UserGroup : ModelBase
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("groupId")]
        public string GroupId { get; set; }
    }
}