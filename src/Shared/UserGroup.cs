using AuthorizationManagement.Shared.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AuthorizationManagement.Shared
{
    public class UserGroup : UserGroupDto, IDocument
    {
        #region ctor
        public UserGroup()
        {

        }

        public UserGroup(UserGroupDto userGroup)
        {
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