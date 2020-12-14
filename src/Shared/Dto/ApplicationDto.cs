﻿using Newtonsoft.Json;
using System;

namespace AuthorizationManagement.Shared.Dto
{
    public class ApplicationDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("userCount")]
        public int UserCount { get; set; }

        [JsonProperty("groupCount")]
        public int GroupCount { get; set; }
    }
}