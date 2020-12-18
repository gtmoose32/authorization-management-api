using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AuthorizationManagement.Api.Models.Internal
{
    public class Group : Models.Group, IDocument
    {
        #region ctor
        public Group()
        {
        }

        public Group(Models.Group group)
        {
            Id = group.Id;
            Name = group.Id;
        }
        #endregion

        [JsonProperty("applicationId")]
        public string ApplicationId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("documentType")]
        public DocumentType DocumentType => DocumentType.Group;

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty(PropertyName = "_ts")]
        public DateTime LastModifiedOn { get; private set; } = DateTime.UtcNow;

        [JsonProperty(PropertyName = "_etag")]
        public string ETag { get; private set; }
    }
}