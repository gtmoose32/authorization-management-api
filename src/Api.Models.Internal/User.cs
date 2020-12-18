using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AuthorizationManagement.Api.Models.Internal
{
    public class User : Models.User, IDocument
    {
        #region ctor
        public User()
        {

        }

        public User(Models.User user)
        {
            Id = user.Id;
            Email = user.Email;
            Enabled = user.Enabled;
            FirstName = user.FirstName;
            LastName = user.LastName;
        }
        #endregion

        [JsonProperty("applicationId")]
        public string ApplicationId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("documentType")]
        public DocumentType DocumentType => DocumentType.User;

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty(PropertyName = "_ts")]
        public DateTime LastModifiedOn { get; private set; } = DateTime.UtcNow;

        [JsonProperty(PropertyName = "_etag")]
        public string ETag { get; private set; }
    }
}