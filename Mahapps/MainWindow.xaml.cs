using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Xml;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using Mahapps.JSONObj;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Timers;
using DDOSDefender.JSONObj;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Net;

namespace Mahapps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        public delegate void degegateHealthStatus(string text);

        // Settings singleton instance
        SettingsSingleton _settings;

        // List of all connected switches
        ObservableCollection<SDNSwitch> listOfSwitchesObsrv;

        List<SDNSwitch> listOfSwitches = new List<SDNSwitch>();

        HealthStatus healthStatus = new HealthStatus();

        SystemUptime sytemUptime = new SystemUptime();

        MemoryStatus memoryStatus = new MemoryStatus();

        FirewallStatus firewallStatus = new FirewallStatus();

        // Thread lock
        private object Threadlock = new object();

        // Query provbe interval
        int probe = 0;

        /// <summary>
        /// 
        /// 
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();



            // get settings from XML
            loadSettingsFromXML("settings.xml");

            // Start checking SDN controller heartbeat
            var statusThread = new Thread(getSDNSwitches);
            statusThread.Start();

            //Start getting SDN Summary

            var summaryThread = new Thread(getSDNSummary);
            summaryThread.Start();

            // Start Healthcheck status
            var healthStatusCheck = new Thread(getSDNHealthly);
            healthStatusCheck.Start();

            DataContext = this;
            listOfSwitchesObsrv = new ObservableCollection<SDNSwitch>();
            listBoxOfSwitches.ItemsSource = listOfSwitchesObsrv;
            listBoxOfSwitches.MouseDoubleClick += ListBoxOfSwitches_MouseDoubleClick;

            SystemUpTimeBox.Text = "" + sytemUptime.UptimeProperty;

            // get firewall status
            var firewallthread = new Thread(getFirewallThread);
            firewallthread.Start();

        }

        private void ListBoxOfSwitches_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Double click");
        }


        // Show Settings. Button in the top-left corner
        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Show();


        }

        // Loading process
        private bool loadSettingsFromXML(String XMLPath)
        {
            if (XMLPath != null)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(XMLPath);
                    XmlNode settingsNode = doc.DocumentElement.SelectSingleNode("/settings/SDNController");



                    //    int Port = int.Parse(settingsNode["Port"].Value);
                    //    int ProbeInterval = int.Parse(settingsNode["Probe"].Value);

                    _settings = SettingsSingleton.Instance;

                    _settings.IpAddress = settingsNode["IPaddress"].InnerText;
                    _settings.Port = settingsNode["Port"].InnerText;
                    _settings.ProbeInterval = settingsNode["Probe"].InnerText;
                    probe = int.Parse(_settings.ProbeInterval);

                }
                catch (NullReferenceException e)
                {
                    MessageBox.Show(e.Message + "\n" + e.StackTrace, "Error 2", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException e)
                {
                    MessageBox.Show(e.Message + "\n" + e.StackTrace, "Error 1", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return true;
            }
            else
            {

                return false;
            }

        }

        //Method for checking SDN heartbeat
        private void getSDNSwitches()
        {

            while (true)
            {
                using (var webClient = new System.Net.WebClient())
                {
                    String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/core/controller/switches/json";
                    Console.WriteLine("URL=" + url);
                    var json = webClient.DownloadString(url);

                    JavaScriptSerializer ser = new JavaScriptSerializer();


                    listOfSwitches = ser.Deserialize<List<SDNSwitch>>(json);

                    try
                    {
                        Dispatcher.Invoke((Action)delegate ()
                        {
                            if (listOfSwitchesObsrv.Count == 0)
                            {
                                foreach (SDNSwitch sw in listOfSwitches)
                                {

                                    listOfSwitchesObsrv.Add(sw);
                                }


                            }


                        });
                    }
                    catch (TaskCanceledException e)
                    {

                    }



                }
                Thread.Sleep(probe * 1000);

            }



        }

        // Get number of links, switches
        private void getSDNSummary()
        {
            String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/core/controller/summary/json";
            while (true)
            {
                using (var webClient = new System.Net.WebClient())
                {
                    var json = webClient.DownloadString(url);

                    String jsonStr = json;
                    String[] jsonArray = jsonStr.Split(',');
                    Console.WriteLine(jsonArray.ToList());
                    if (jsonArray.Length >= 4)
                    {
                        Regex r = new Regex(@"(\d+)", RegexOptions.IgnoreCase);
                        Match match1 = r.Match(jsonArray[0]);
                        numberOfSwitchesTextBox.Dispatcher.BeginInvoke((Action)(() => numberOfSwitchesTextBox.Text = "" + match1.Value));
                        Match match2 = r.Match(jsonArray[1]);
                        numberOfQuarantinePortsBox.Dispatcher.BeginInvoke((Action)(() => numberOfQuarantinePortsBox.Text = "" + match2.Value));
                        Match match3 = r.Match(jsonArray[2]);
                        numberOfISLBox.Dispatcher.BeginInvoke((Action)(() => numberOfISLBox.Text = "" + match2.Value));
                        Match match4 = r.Match(jsonArray[3]);
                        numberOfHosts.Dispatcher.BeginInvoke((Action)(() => numberOfHosts.Text = match4.Value));
                    }

                }

                Thread.Sleep(probe * 1000);
            }

        }

        // Get SDN controller status
        private void getSDNHealthly()
        {
            while (true)
            {
                using (var webClient = new System.Net.WebClient())
                {
                    String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/core/health/json";
                    var json = webClient.DownloadString(url);
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    healthStatus = ser.Deserialize<HealthStatus>(json);

                    String url2 = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/core/system/uptime/json";
                    sytemUptime = ser.Deserialize<SystemUptime>(webClient.DownloadString(url2));

                    String url3 = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/core/memory/json";
                    memoryStatus = ser.Deserialize<MemoryStatus>(webClient.DownloadString(url3));



                    // Update GUI elements
                    if (healthStatus.HealthyStatus)
                    {
                        isHealthBox.Dispatcher.BeginInvoke((Action)(() => isHealthBox.Text = "OK"));
                    }
                    else
                    {
                        isHealthBox.Dispatcher.BeginInvoke((Action)(() => isHealthBox.Text = "NOK"));
                    }

SystemUpTimeBox.Dispatcher.BeginInvoke((Action)(() => SystemUpTimeBox.Text = "" + sytemUptime.systemUptimeMsec));
                    FreeMemoryBox.Dispatcher.BeginInvoke((Action)(() => FreeMemoryBox.Text = "" + memoryStatus.FreeProperty));
                    TotalMemoryBox.Dispatcher.BeginInvoke((Action)(() => TotalMemoryBox.Text = "" + memoryStatus.TotalPropertry));
                    // End of GUI update
                }

                Thread.Sleep(probe* 1000);
            }
        }

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
                        FirewallToggleButton.Dispatcher.BeginInvoke((Action)(() => FirewallToggleButton.Content = "enable")); 
                    }
                    else
                    {
                        FirewallStatusText.Dispatcher.BeginInvoke((Action)(() => FirewallStatusText.Text = "running"));
                        FirewallToggleButton.Dispatcher.BeginInvoke((Action)(() => FirewallToggleButton.Content = "disable"));
                    }
                    
                }
        Thread.Sleep(probe * 1000);
    }

}

        // Button action to enable/disable firewall functionality
        private void FirewallToggleButton_Click(object sender, RoutedEventArgs e)
        {

            String FWaction = "enable";

            if (FirewallStatusText.Text.Contains("running"))
            {
                FWaction = "disable";
            }
            else
            {
                FWaction = "enable";
            }
            String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/firewall/module/" + FWaction+ "/json";
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "PUT";
            request.AllowWriteStreamBuffering = false;
            request.ContentType = "application/json";
            request.Accept = "Accept=application/json";
            request.SendChunked = false;
            request.ContentLength = 0;
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
//                writer.Write();
            }
            var response = request.GetResponse() as HttpWebResponse;

            MessageBox.Show("Response: " + response.StatusCode.ToString());
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
{

    System.Windows.Data.CollectionViewSource healthStatusViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("healthStatusViewSource")));
    // Load data by setting the CollectionViewSource.Source property:
    // healthStatusViewSource.Source = [generic data source]
}


// Forced closing app
private void MetroWindow_Closing(object sender, CancelEventArgs e)
{
    Environment.Exit(0);
}

    }
}
