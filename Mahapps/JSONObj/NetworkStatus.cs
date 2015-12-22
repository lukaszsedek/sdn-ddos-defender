using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahapps.JSONObj
{

    class NetworkStatus
    {
        [JsonProperty(PropertyName ="# Switches")]
        public int Switches { get; set; }
        [JsonProperty(PropertyName = "# quarantine ports")]
        public int quarantineports { get; set; }
        public int interswitchlinks { get; set; }
        public int hosts { get; set; }

        public override string ToString()
        {
            return "Switches=" + Switches + " ports=" + quarantineports + " interswitchlinks=" + interswitchlinks + " hosts=" + hosts;
        }
    }
}

