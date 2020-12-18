using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AuthorizationManagement.Api.Models.Internal
{
    [ExcludeFromCodeCoverage]
    public class Application : Models.Application, IDocument
    {
        #region ctor
        public Application()
        {
        }

        public Application(Models.Application app)
        {
            Name = app.Name;
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