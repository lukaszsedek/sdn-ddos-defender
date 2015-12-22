using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahapps.JSONObj
{ 


    public class ControllerSwitchesSummary
    {
        public SDNSwitch[] Property1 { get; set; }
    }

    public class SDNSwitch : INotifyPropertyChanged
    {

        [JsonProperty(PropertyName = "inetAddress")]
        public string _inetadd { get; set; } 

        
        public string inetAddress {
            get { return _inetadd; } 
            set
            {
                if (this._inetadd != value)
                    this.NotifyPropertyChanged("inetAddress");
            } }
        [JsonProperty(PropertyName = "connectedSince")]
        public long connectedSince { get; set; }
        [JsonProperty(PropertyName = "switchDPID")]
        public string switchDPID { get; set; }

     
        
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return "IP = " + inetAddress + " SWITCH ID= " + switchDPID;
        }


    }

}
