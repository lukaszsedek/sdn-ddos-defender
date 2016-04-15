using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDOSDefender.JSONObj
{
    class ACL: INotifyPropertyChanged
    {
        public int id { get; set; }
        public string nw_src { get; set; }
        public string nw_dst { get; set; }
        public int nw_src_prefix { get; set; }
        public int nw_src_maskbits { get; set; }
        public int nw_dst_prefix { get; set; }
        public int nw_dst_maskbits { get; set; }
        public int nw_proto { get; set; }
        public int tp_dst { get; set; }
        public string action { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendFormat("ID {0}", id);
            return strBuilder.ToString();
        }
    }
}



