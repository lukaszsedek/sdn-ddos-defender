using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDOSDefender.JSONObj
{
    public class SDNFlowTable: INotifyPropertyChanged
    {
                
        public ObservableCollection<Flow> flows { get; set; }
        
        /*
            Flow from FlowTable
        */
        public class Flow
        {
            public string switchID { get; set; }
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
            public string udp_src { get; set; }
            public string udp_dst { get; set; }
            public string tcp_src { get; set; }
            public string tcp_dst { get; set; }

            public override string ToString()
            {
                StringBuilder result = new StringBuilder();
                
                // port
                if(in_port != "")
                {
                    result.AppendLine("Ingress port \t" + in_port);
                }
                else
                {
                    result.AppendLine("Ingress port \t" );
                }
                // IPv4.src
                if(ipv4_src != "")
                {
                    result.AppendLine("IPv4 source  \t" + ipv4_src);
                }
                else
                {
                    result.AppendLine("IPv4 source  \t any");
                }
                // IPv4.dst
                if (ipv4_dst != "")
                {
                    result.AppendLine("IPv4 destination  \t" + ipv4_dst);
                }
                else
                {
                    result.AppendLine("IPv4 source  \t any");
                }
                // UDP
                if (udp_dst != null && udp_src != null)
                {
                    result.AppendLine("UDP dst port\t" + udp_dst);
                    result.AppendLine("UDP src port\t" + udp_src);

                }
                // TCP
                if(tcp_dst != null && tcp_src != null)
                {
                    result.AppendLine("TCP dst port\t" + tcp_dst);
                    result.AppendLine("TCP src port\t" + tcp_src);
                }
                return result.ToString();
            }
        }

        public class Instructions
        {
            public Instruction_Apply_Actions instruction_apply_actions { get; set; }

            public override string ToString()
            {
                if (instruction_apply_actions != null)
                    return instruction_apply_actions.actions;
                else
                    return "drop";
            }
        }

        public class Instruction_Apply_Actions
        {
            public string actions { get; set; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}










