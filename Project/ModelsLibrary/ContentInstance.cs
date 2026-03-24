using Newtonsoft.Json;
using System;

namespace ModelsLibrary
{
    public class ContentInstance
    {
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        [JsonProperty("content-type")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("creation-datetime")]
        public DateTime CreatedAt { get; set; }
    }
}
