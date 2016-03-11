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

namespace Mahapps
{
    public partial class MainWindow
    {
        // Checking Firewall availability
        private void getFirewallThread()
        {
            while (true)
            {
                using (var webClient = new System.Net.WebClient())
                {
                    String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/firewall/module/status/json";
                    var json = webClient.DownloadString(url);
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    firewallStatus = ser.Deserialize<FirewallStatus>(json);
                    if (firewallStatus.result.Equals("firewall disabled"))
                    {
                        FirewallStatusText.Dispatcher.BeginInvoke((Action)(() => FirewallStatusText.Text = "OFF"));
                        // FirewallToggleButton.Dispatcher.BeginInvoke((Action)(() => FirewallToggleButton.Content = "enable"));
                        Dispatcher.BeginInvoke((Action)(() => eventList.Add(new EventItem("Firewall is off", EventItem.SEVERITY.Critical))));
                    }
                    else
                    {
                        FirewallStatusText.Dispatcher.BeginInvoke((Action)(() => FirewallStatusText.Text = "running"));
                        Dispatcher.BeginInvoke((Action)(() => eventList.Add(new EventItem("Firewall is running", EventItem.SEVERITY.Informational))));

                    }

                }
                Thread.Sleep(probe * 1000);
            }

        }
        // Update FW rules table
        private void updateFWrulesThread()
        {
            //http://192.168.56.101:8080/wm/firewall/rules/json

            while (true)
            {
                try
                {
                    using (var webClient = new System.Net.WebClient())
                    {
                        String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/firewall/rules/json";
                        var json = webClient.DownloadString(url);
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        this.FWrules = ser.Deserialize<ObservableCollection<FWEntry>>(json);
                        Dispatcher.BeginInvoke(
                            (Action)(() => FWGrid.ItemsSource = FWrules)

                            );



                    }
                }
                catch (WebException e)
                {
                    MessageBox.Show(_settings.IpAddress + " address is unreachable", "Error 4", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }



                Thread.Sleep(probe * 1000);
            }
        }

    }
}
