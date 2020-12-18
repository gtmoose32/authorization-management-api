using Newtonsoft.Json;
using System;

namespace AuthorizationManagement.Api.Models
{
    public abstract class ModelBase
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}