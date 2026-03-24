using Newtonsoft.Json;
using System;

namespace ModelsLibrary
{
    public class Application
    {
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        [JsonProperty("creation-datetime")]
        public DateTime CreatedAt { get; set; }
    }
}
