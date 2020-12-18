using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AuthorizationManagement.Api.Models.Internal
{
    public interface IDocument
    {
        [JsonProperty("id")]
        string Id { get; set; }

        [JsonProperty("applicationId")]
        string ApplicationId { get; }

        [JsonProperty("documentType")]
        [JsonConverter(typeof(StringEnumConverter))]
        DocumentType DocumentType { get; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty(PropertyName = "_ts")]
        DateTime LastModifiedOn { get; }

        [JsonProperty(PropertyName = "_etag")]
        string ETag { get; }
    }
}
