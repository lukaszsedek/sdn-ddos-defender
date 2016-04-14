using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using System.Xml;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using Mahapps.JSONObj;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Text;

namespace Mahapps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        public delegate void degegateHealthStatus(string text);
        
        SettingsSingleton _settings; // Settings singleton instance        
        ObservableCollection<SDNSwitch> listOfSwitchesObsrv; // List of all connected switches
        List<SDNSwitch> listOfSwitches          = new List<SDNSwitch>();
        HealthStatus healthStatus               = new HealthStatus();
        SystemUptime sytemUptime                = new SystemUptime();
        MemoryStatus memoryStatus               = new MemoryStatus();
        FirewallStatus firewallStatus           = new FirewallStatus();
        ObservableCollection<FWEntry> FWrules   = new ObservableCollection<FWEntry>();

        // Thread lock
        private object Threadlock               = new object();

        // Query provbe interval
        int probe = 0;

        ObservableCollection<EventItem> eventList;


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

            // Topology thread
            DataContext = this;
            listOfSwitchesObsrv = new ObservableCollection<SDNSwitch>();
            listBoxOfSwitches.ItemsSource = listOfSwitchesObsrv;
            listBoxOfSwitches.MouseDoubleClick += ListBoxOfSwitches_MouseDoubleClick;

            SystemUpTimeBox.Text = "" + sytemUptime.UptimeProperty;

            // get firewall status
            var firewallthread = new Thread(getFirewallThread);
            firewallthread.Start();

            // Firewall rules thread
            var updateFWrules = new Thread(updateFWrulesThread);
            updateFWrules.Start();

            statsGrid.ItemsSource = stats;
            var getStats = new Thread(getStatsThread);
            getStats.Start();
            
            // Collection of logs
            eventList = new ObservableCollection<EventItem>();
            eventList.Add(new EventItem { Severity=EventItem.SEVERITY.Debug, Message = "SDN DDOS mitigation application is up and running" });
            EvenGrid.ItemsSource = eventList;
            FWGrid.ItemsSource = FWrules;

            // Load DDOS table Thread
            DDOSGrid.ItemsSource = DDosTable;
            loadDDOSRules("ddos_config.xml");
            var ddosCheckerThread = new Thread(DDOSThread);
            ddosCheckerThread.Start();

            // FlowTable Thread
            FlowTableGird.ItemsSource = sdnFlowTable.flows;
            var flowTableThread = new Thread(FlowTableThread);
            flowTableThread.Start();

        }

        // Double click on Switch list
        // create explicite ALLOW Firewall rule for this switch
        private void ListBoxOfSwitches_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // PUT a new explicite allow rule by creating background thread
            String switchPID = ((SDNSwitch)listBoxOfSwitches.SelectedItem).switchDPID;
            Thread thread = new Thread(() => addImplicitAllowFWRule(switchPID));
            thread.Start();
        }


        // Show Settings. Button in the top-left corner
        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Show();
        }

        // Loading process of settings XML file
        private bool loadSettingsFromXML(String XMLPath)
        {
            String tempStr = XMLPath;
            if (XMLPath != null)
            {
                if(!File.Exists(XMLPath))
                {
                    OpenFileDialog openFileDialogXML = new OpenFileDialog();
                    // Set filter options and filter index.
                    openFileDialogXML.Filter = "Text Files (.xml)|*.xml|All Files (*.*)|*.*";
                    openFileDialogXML.FilterIndex = 1;
                    openFileDialogXML.Multiselect = true;

                    bool? userClickedOK = openFileDialogXML.ShowDialog();
                    // Process input if the user clicked OK.
                    if (userClickedOK == true)
                    {
                        tempStr = openFileDialogXML.FileName;
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
                
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(tempStr);
                    XmlNode settingsNode = doc.DocumentElement.SelectSingleNode("/settings/SDNController");

                    _settings = SettingsSingleton.Instance;

                    _settings.IpAddress = settingsNode["IPaddress"].InnerText;
                    _settings.Port = settingsNode["Port"].InnerText;
                    _settings.ProbeInterval = settingsNode["Probe"].InnerText;
                    probe = int.Parse(_settings.ProbeInterval);

                    // Try to ping IP address of controller
                    Ping pingSender = new Ping();
                    IPAddress address = IPAddress.Parse(_settings.IpAddress);
                    PingReply reply = pingSender.Send(address);
                    if (reply.Status == IPStatus.Success)
                    {
                        using (var webClient = new System.Net.WebClient())
                        {
                            try {
                                String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/core/health/json";
                                var json = webClient.DownloadString(url);
                                JavaScriptSerializer ser = new JavaScriptSerializer();
                                healthStatus = ser.Deserialize<HealthStatus>(json);

                            }
                            catch (WebException e)
                            {
                                MessageBox.Show("Unable to connect REST service. Please chech Floodlight application or change IP address in settings.xml file  \n" + e.Message + "\n" + e.StackTrace, "Error 5", MessageBoxButton.OK, MessageBoxImage.Error);
                                Environment.Exit(0);
                            }
                            } // using...
                            
                    } // if... 
                    else
                    {
                        MessageBox.Show(_settings.IpAddress + " address is unreachable", "Error 4", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                        
                    }

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

 
        // Get number of links, switches....
        // Needed manulaly parse JSON because of wrong format of JSON file. It contains invalid characters as "#"
        // Parsed by regexp
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
        // Update Controller Health status
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
                    TimeSpan time = TimeSpan.FromMilliseconds(sytemUptime.systemUptimeMsec);
                    string uptime = time.ToString(@"hh\:mm\:ss\.fff");

                    SystemUpTimeBox.Dispatcher.BeginInvoke((Action)(() => SystemUpTimeBox.Text = "" + uptime));
                    FreeMemoryBox.Dispatcher.BeginInvoke((Action)(() => FreeMemoryBox.Text = "" + memoryStatus.FreeProperty));
                    TotalMemoryBox.Dispatcher.BeginInvoke((Action)(() => TotalMemoryBox.Text = "" + memoryStatus.TotalPropertry));
                    // End of GUI update
                }

                Thread.Sleep(probe* 1000);
            }
        }

        // Button action to enable statistics
        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/statistics/config/enable/json";
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                String postData = "";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Console.WriteLine(responseFromServer);
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (WebException exception)
            {
                addLogUI(exception.StackTrace, 0);
            }
        }

        // Button action to enable/disable firewall functionality
        private void FirewallToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(turnOnFirewall);            
            thread.Start();
        }

        /* 
        By default Floodlight controller has firewall module disabled
        When you turn on firewall it creates by default implicit DENY ALL rules for all switches
        To avoid dropping packets, please be sure you apply EXPLICIT ALLOW all 
        This method enables firewall
        */
        public void turnOnFirewall()
        {
            String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/firewall/module/enable/json";
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "PUT";
                String postData = "";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Console.WriteLine(responseFromServer);
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

        // method for adding a new event in Log Events
        // _msg message to put 
        // severity 0... 6 0 - alert, 6 - Debug. Syslog convention
        public void addLogUI(string _msg, int severity)
        {
            EventItem.SEVERITY _sev = (EventItem.SEVERITY)severity;
            Dispatcher.BeginInvoke((Action)(() => eventList.Add(new EventItem(_msg, _sev))));
        }
              
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

           System.Windows.Data.CollectionViewSource healthStatusViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("healthStatusViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // healthStatusViewSource.Source = [generic data source]
}

        private void AddDDOS_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Set filter options and filter index.
            openFileDialog.Filter = "Text Files (.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = true;

            bool? userClickedOK = openFileDialog.ShowDialog();
            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                DDosTable = new ObservableCollection<DDOSDefender.JSONObj.DDOSTable>();
                loadDDOSRules(openFileDialog.FileName);
            }
        }
        
        // Forced closing app
        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }


    }
}
