using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahapps.JSONObj
{
    class HealthStatus: INotifyPropertyChanged
    {
        [JsonProperty(PropertyName = "healthy")]
        public bool healthy;

        
        public bool HealthyStatus
        {
            get { return healthy; }
            set
            {
                if (this.healthy != value)
                    this.NotifyPropertyChanged("Healthy");
            }
        }
        
        

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return (healthy == true) ? "true" : "not health";
        }
    }
}
