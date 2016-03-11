using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahapps.JSONObj
{
    class EventItem: INotifyPropertyChanged
    {
        //Syslog standard
        public enum SEVERITY
        {
            Emergency=0,
            Alert,
            Critical,
            Error,
            Warning,
            Notice,
            Informational,
            Debug
        }

        static private int counter = 0;
        int id =0;
        SEVERITY sev;
        DateTime date;
        String message;

        public int ID {
            get { return id; }
            set { id = value;  }
        }

        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        public SEVERITY Severity
        {
            get { return sev; }
            set { sev = value; }
        }

        public String Message
        {
            get { return message; }
            set { message = value; }
        }

        public EventItem()
        {
            ID = counter;
            date = DateTime.Now;
        }
        public EventItem( string _message, SEVERITY _sev)
        {
            counter++;
            id = counter;
            message = _message;
            date = DateTime.Now;
            sev = _sev;
        }

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
