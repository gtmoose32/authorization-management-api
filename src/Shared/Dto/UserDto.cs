using Newtonsoft.Json;
using System;

namespace AuthorizationManagement.Shared.Dto
{
    public class UserDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

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