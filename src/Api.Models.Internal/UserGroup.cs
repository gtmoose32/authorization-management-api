using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AuthorizationManagement.Api.Models.Internal
{
    [ExcludeFromCodeCoverage]
    public class UserGroup : Models.UserGroup, IDocument
    {
        [JsonProperty("applicationId")]
        public string ApplicationId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("documentType")]
        public DocumentType DocumentType => DocumentType.UserGroup;

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty(PropertyName = "_ts")]
        public DateTime LastModifiedOn { get; private set; } = DateTime.UtcNow;

        [JsonProperty(PropertyName = "_etag")]
        public string ETag { get; private set; }
    }
}