﻿using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace AuthorizationManagement.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class Application : ModelBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}