using Newtonsoft.Json;
using System;

namespace AuthorizationManagement.Api.Models
{
    public class UserGroup
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("groupId")]
        public string GroupId { get; set; }
    }
}