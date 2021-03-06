﻿using Agilefantasy.Common;
using Newtonsoft.Json;

namespace Agilefantasy
{
    public class AgilefantBacklogProductSummary : AgilefantBase
    {
        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("product")]
        public bool Product { get; private set; }

        [JsonProperty("standAlone")]
        public bool StandAlone { get; private set; }
    }
}
