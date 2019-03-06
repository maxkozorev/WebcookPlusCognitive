using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Models
{
    class MetadataExampleModel
    {
        public class Face
        {
            [JsonProperty("value")]
            public string value { get; set; }
        }
        public class Description
        {
            [JsonProperty("value")]
            public string description { get; set; }
        }
        public class Tags
        {
            [JsonProperty("value")]
            public IList<string> value { get; set; }
        }

        public class Metadata
        {
            [JsonProperty("5")]
            public Description description { get; set; }

            [JsonProperty("120")]
            public Face facename { get; set; }

            [JsonProperty("25")]
            public Tags tagsname { get; set; }
        }

        public class Send_Metadata
        {
            [JsonProperty("metadata")]
            public Metadata metadata { get; set; }
        }
    }
}
