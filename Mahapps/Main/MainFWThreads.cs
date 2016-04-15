using DDOSDefender;
using Mahapps.JSONObj;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
                        
                        firewallTile.Dispatcher.BeginInvoke(d,  new object[] { "OFF" });   
                    }
                    else
                    {
                        firewallTile.Dispatcher.BeginInvoke(d, new object[] { "OK" });
                    }
                }
                Thread.Sleep(probe * 1000);
            }
        }

        // Update Firewall rules table
        // Refreshing UI Firewall Rules
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
            firewallTile.Count = text;
        }

        // Add implicit Allow all firewall rule to switch
        private void addImplicitAllowFWRule(String switchPID)
        {
            // Exit if switchID is empty
            if (switchPID == null)
                return;

            // Build POST HTTPP request
            String urlFirewall = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/firewall/rules/json";
            try
            {
                WebRequest request = WebRequest.Create(urlFirewall);
                request.Method = "POST";
                String postData = "{\"switchid\":\"" + switchPID + "\"}";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                addLogUI("explicite allow all firewall rule for " + switchPID + " =  " + ((HttpWebResponse)response).StatusDescription, 5);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();

            }
            catch (WebException exception)
            {
                Console.WriteLine(exception.StackTrace);
                addLogUI(exception.StackTrace, 0);
            }
            
        }

        public void addNewFirewallButton_click(object sender, RoutedEventArgs e)
        {

            // Testing purpose 
            addCustomFirewallRule("00:00:00:00:00:00:00:04", "DENY", "10.20.11.0", "10.2.2.2", "UDP", "1111", "2222");
            addCustomFirewallRule("00:00:00:00:00:00:00:04", "DENY", "10.20.11.1", "10.2.2.2", "UDP", "1111", "2222");
            addCustomFirewallRule("00:00:00:00:00:00:00:04", "DENY", "10.20.11.2", "10.2.2.2", "UDP", "1111", "2222");
            addCustomFirewallRule("00:00:00:00:00:00:00:04", "DENY", "10.20.11.3", "10.2.2.2", "UDP", "1111", "2222");

        }

        /*
            send post to sdn controller to deny or allow particular flow
        */
        public void addCustomFirewallRule(String switchID, String action, String ipSource, String ipDst, String nw_proto, String tp_src, String tp_dst)
        {
            if (!(action.Equals("ALLOW") || action.Equals("DENY")))
                return;
            if (!(nw_proto.Equals("TCP") || nw_proto.Equals("UDP")))
                return;
            if (ipSource == "" || ipDst == "" || tp_dst == "" || tp_src == "")
                return;

            // 1. Build JSON message
            
            string json = "{  \"src-ip\": \"" + ipSource + "\", \"dst-ip\": \"" + ipDst + "/32\", \"nw-proto\":\"" + nw_proto + "\", \"tp-src\":\"" + tp_src +"\", \"tcp-dst\":\"" + tp_dst + "\", \"action\":\""  +  action + "\" }";
            // 2. send POST
            String urlFirewall = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/firewall/rules/json";
            try
            {
                WebRequest request = WebRequest.Create(urlFirewall);
                request.Method = "POST";
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                addLogUI("Add firewall rule status " + json + " \t" +  ((HttpWebResponse)response).StatusDescription, 5);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();

            }
            catch (WebException exception)
            {
                Console.WriteLine(exception.StackTrace);
                addLogUI(exception.StackTrace, 0);
            }
        }

        
    }
}
