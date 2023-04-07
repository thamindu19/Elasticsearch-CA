using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Domain.Entities.Documents
{
    public class Document
    {
        [JsonProperty("file_id")]
        public Guid fileId { get; set; }
        
        [JsonProperty("file_name")]
        public string fileName { get; set; }
        
        public string[] tags { get; set; }
        
        [JsonProperty("access_roles")]
        public string[] accessRoles { get; set; }
        
        public string message { get; set; }
    }
}
