using Mahapps.JSONObj;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahapps
{
    class AppModel : INotifyPropertyChanged
    {
      
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
