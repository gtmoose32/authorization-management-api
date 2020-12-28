using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AuthorizationManagement.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class User : UserInfo
    {
        [JsonProperty("groups")]
        public IList<Group> Groups { get; set; } = new List<Group>();
    }
}