using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDOSDefender.JSONObj
{
    public class FlowTable
    {
                
        public Flow[] flows { get; set; }
        

        public class Flow
        {
            public string version { get; set; }
            public string cookie { get; set; }
            public string tableId { get; set; }
            public string packetCount { get; set; }
            public string byteCount { get; set; }
            public string durationSeconds { get; set; }
            public string durationNSeconds { get; set; }
            public string priority { get; set; }
            public string idleTimeoutSec { get; set; }
            public string hardTimeoutSec { get; set; }
            public string flags { get; set; }
            public Match match { get; set; }
            public Instructions instructions { get; set; }
        }

        public class Match
        {
            public string in_port { get; set; }
            public string eth_dst { get; set; }
            public string eth_src { get; set; }
            public string eth_type { get; set; }
            public string ipv4_src { get; set; }
            public string ipv4_dst { get; set; }
        }

        public class Instructions
        {
            public Instruction_Apply_Actions instruction_apply_actions { get; set; }
        }

        public class Instruction_Apply_Actions
        {
            public string actions { get; set; }
        }

    }
}
