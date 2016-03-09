using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDOSDefender.JSONObj
{
    class SystemUptime: INotifyPropertyChanged
    {

        [JsonProperty(PropertyName = "systemUptimeMsec")]
        public long systemUptimeMsec { get; set; }

        
        public long UptimeProperty
        {
            get
            {
                return systemUptimeMsec;
            }
            set
            {
                if(systemUptimeMsec != value)
                    this.NotifyPropertyChanged("UptimeProperty");
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
