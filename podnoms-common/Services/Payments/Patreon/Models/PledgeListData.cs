using System.Collections.Generic;
using Newtonsoft.Json;

namespace PodNoms.Common.Services.Payments.Patreon.Models {
    public class PledgeListData {
        [JsonProperty(PropertyName = "data")]
        public List<Pledge> Pledges { get; set; }

        [JsonProperty(PropertyName = "links")]
        public Links Links { get; set; }
    }
}
