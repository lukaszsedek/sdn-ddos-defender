using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahapps
{
    class SettingsSingleton : INotifyPropertyChanged
    {
        // Members
        public  String IpAddress { get; set; }
        public  String Port { get; set; }
        public  String ProbeInterval { get; set; }

        // Singleton instance
        private static SettingsSingleton instance;

        // Synchronized plain object
        private static object syncRoot = new Object();

        public event PropertyChangedEventHandler PropertyChanged;

        // fake ctor
        private SettingsSingleton() { }

        // get method
        public static SettingsSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {

                        instance = new SettingsSingleton();
                    }
                }
                return instance;
            }

            
        }


        // ToString override
        public override string ToString()
        {
            return "IP address=" + IpAddress + ",Port=" + Port + ",Probe=" + ProbeInterval ;
        }

    }
}
