using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AuthorizationManagement.Api.Models.Internal
{
    public class UserGroup : Models.UserGroup, IDocument
    {
        #region ctor
        public UserGroup()
        {

        }

        public UserGroup(string applicationId, Models.UserGroup userGroup)
        {
            ApplicationId = applicationId;
            Id = userGroup.Id;
            GroupId = userGroup.GroupId;
            UserId = userGroup.UserId;
        } 
        #endregion

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