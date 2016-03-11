using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDOSDefender.JSONObj
{
    class DDOSTable: INotifyPropertyChanged
    {
        public enum action{
            DROP=0,
            ALERT=1
        }
        private String id;
        public String ID
        {
            get { return id; }
            set { id = value; }
        }

        private String switchID;
        public String SwitchID
        {
            get { return switchID;  }
            set { switchID = value; }
        }

        private String port;
        public String Port
        {
            get { return port; }
            set { port = value; }
        }

        private String ipDST;
        public String IPDST
        {
            get { return ipDST; }
            set { ipDST = value; }
        }
        private String maxTXBPS;
        public String MAX_TX_BPS
        {
            get { return maxTXBPS; }
            set { maxTXBPS = value; }
        }
        private String maxRXBPS;
        public String MAX_RX_BPS
        {
            get { return maxRXBPS; }
            set { maxRXBPS = value; }
        }
        private action _action;
        public action Action
        {
            get { return _action; }
            set { _action = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
