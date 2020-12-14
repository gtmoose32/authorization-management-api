using Newtonsoft.Json;
using System;

namespace AuthorizationManagement.Shared.Dto
{
    public class GroupDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}