﻿using Newtonsoft.Json;
using System;

namespace AuthorizationManagement.Api.Models
{
    public class Group
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}