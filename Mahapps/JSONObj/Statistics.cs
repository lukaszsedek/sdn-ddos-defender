using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DDOSDefender.JSONObj
{
    [DataContract]
    class Statistics
    {
        [DataMember(Name = "dpid")]
        public string dpid { get; set; }
        [DataMember(Name = "port")]
        public string port { get; set; }
        [DataMember(Name = "updated")]
        public string updated { get; set; }
        [DataMember(Name = "bits-per-second-rx")]
        private string bitspersecondrx { get; set; }
        
        public String BPPSRX
        {
            get { return bitspersecondrx; }
            set { bitspersecondrx = value; }
        }
        [DataMember(Name = "bits-per-second-tx")]
        private string bitspersecondtx { get; set; }

        public String BPSTX
        {
            get { return bitspersecondtx;  }
            set { bitspersecondtx = value; }
        }
        public override string ToString()
        {
            return dpid + " " + port + " " + updated + " " + BPPSRX + " " + bitspersecondtx;
        }
    }
}
