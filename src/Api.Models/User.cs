using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace AuthorizationManagement.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class User : ModelBase
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        
    }
}