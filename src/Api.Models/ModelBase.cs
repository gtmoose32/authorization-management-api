using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AuthorizationManagement.Api.Models
{
    [ExcludeFromCodeCoverage]
    public abstract class ModelBase
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}