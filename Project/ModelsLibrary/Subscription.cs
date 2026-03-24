using Newtonsoft.Json;
using System;

namespace ModelsLibrary
{
    public class Subscription
    {
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        [JsonProperty("evt")]
        public int Evt { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("creation-datetime")]
        public DateTime CreatedAt { get; set; }
    }
}
