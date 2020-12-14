using AuthorizationManagement.Shared.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AuthorizationManagement.Shared
{
    public class Application : ApplicationDto, IDocument
    {
        #region ctor
        public Application()
        {
        }

        public Application(ApplicationDto appDto)
        {
            Name = appDto.Name;
        } 
        #endregion

        [JsonProperty("applicationId")]
        public string ApplicationId => Id;

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("documentType")]
        public DocumentType DocumentType => DocumentType.Application;

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty(PropertyName = "_ts")]
        public DateTime LastModifiedOn { get; private set; } = DateTime.UtcNow;

        [JsonProperty(PropertyName = "_etag")]
        public string ETag { get; private set; }

    }
}