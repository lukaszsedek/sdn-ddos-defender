using Mahapps.JSONObj;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;

/*
    Firewall part of this project. Part of Main class
*/
namespace Mahapps
{
    public partial class MainWindow
    {
        // UI delegates
        public delegate void delUpdateUIFirewallStatusText(String fwStatus);

        // Checking Firewall availability
        private void getFirewallThread()
        {
            while (true)
            {
                using (var webClient = new System.Net.WebClient())
                {
                    String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/firewall/module/status/json";
                    var json = webClient.DownloadString(url);
                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    firewallStatus = javaScriptSerializer.Deserialize<FirewallStatus>(json);
                    delUpdateUIFirewallStatusText d = new delUpdateUIFirewallStatusText(setFwStatusText);
                    if (firewallStatus.result.Equals("firewall disabled"))
                    {
                        
                        FirewallStatusText.Dispatcher.BeginInvoke(d,  new object[] { "OFF" });   
                    }
                    else
                    {
                        FirewallStatusText.Dispatcher.BeginInvoke(d, new object[] { "UP" });
                    }
                }
                Thread.Sleep(probe * 1000);
            }
        }
        // Update FW rules table
        private void updateFWrulesThread()
        {
            while (true)
            {
                try
                {
                    using (var webClient = new System.Net.WebClient())
                    {
                        String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/firewall/rules/json";
                        var json = webClient.DownloadString(url);
                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        this.FWrules = javaScriptSerializer.Deserialize<ObservableCollection<FWEntry>>(json);
                        Dispatcher.BeginInvoke((Action)(() => FWGrid.ItemsSource = FWrules));
                    }
                }
                catch (WebException e)
                {
                    MessageBox.Show(_settings.IpAddress + " address is unreachable\n" + e.StackTrace, "Error 4", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }

                Thread.Sleep(probe * 1000);
            }
        }

        // Update FriwallStatusText UI field
        private void setFwStatusText(String text)
        {
            FirewallStatusText.Text = text;
        }
    }
}
