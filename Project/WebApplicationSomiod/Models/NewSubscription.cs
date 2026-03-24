using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplicationSomiod.Models
{
    public class NewSubscription
    {
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        [JsonProperty("evt")]
        public int Evt { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
    }
}