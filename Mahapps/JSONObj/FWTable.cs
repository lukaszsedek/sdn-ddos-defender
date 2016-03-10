using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDOSDefender.JSONObj
{

    public class FirewallRules
    {
        public ObservableCollection<FWEntry> FWRules { get; set; }
    }

    public class FWEntry: INotifyPropertyChanged
    {
        
        private int _ruleid { get; set; }
        [JsonProperty("ruleid")]
        public int RuleID
        {
            get { return _ruleid; }
            set { _ruleid = value; }
        }
        public string dpid { get; set; }
        public int in_port { get; set; }
        public string dl_src { get; set; }
        public string dl_dst { get; set; }
        public int dl_type { get; set; }
        public string nw_src_prefix { get; set; }
        public int nw_src_maskbits { get; set; }
        public string nw_dst_prefix { get; set; }
        public int nw_dst_maskbits { get; set; }
        public int nw_proto { get; set; }
        public int tp_src { get; set; }
        public int tp_dst { get; set; }
        public bool any_dpid { get; set; }
        public bool any_in_port { get; set; }
        public bool any_dl_src { get; set; }
        public bool any_dl_dst { get; set; }
        public bool any_dl_type { get; set; }
        public bool any_nw_src { get; set; }
        public bool any_nw_dst { get; set; }
        public bool any_nw_proto { get; set; }
        public bool any_tp_src { get; set; }
        public bool any_tp_dst { get; set; }
        public int priority { get; set; }
        public string action { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
