using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDOSDefender.JSONObj
{
    class MemoryStatus: INotifyPropertyChanged
    {

        public int total { get; set; }
        public int free { get; set; }

        public int TotalPropertry
        {
            get { return total;  }
            set
            {
                if (this.total != value)
                    this.NotifyPropertyChanged("TotalPropertry");
            }
        }

        public int FreeProperty
        {
            get { return free; }
            set
            {
                if (this.free != value)
                    this.NotifyPropertyChanged("FreeProperty");
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }



        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return "FREE=" + FreeProperty + " TOTAL=" + TotalPropertry;
        }
    }
}
