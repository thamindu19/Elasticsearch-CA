using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Common;
using Newtonsoft.Json;

namespace Domain.Entities.Documents
{
    public class Document : BaseEntity
    {
        [JsonProperty("file_id")]
        public Guid FileId { get; set; }
        
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        
        [JsonProperty("tags")]
        public string[] Tags { get; set; }
        
        [JsonProperty("access_roles")]
        public string[] AccessRoles { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("group")]
        public string Group { get; set; }
    }
}
