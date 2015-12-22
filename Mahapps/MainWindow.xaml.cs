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
        // Number of links,switches...
        NetworkStatus networkStatus;

        // 
        
        HealthStatus healthStatus = new HealthStatus();


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
            


            // Thread safe
            lock (Threadlock)
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

                        Dispatcher.Invoke((Action)delegate()
                        {
                            if (listOfSwitchesObsrv.Count == 0)
                            {
                                foreach (SDNSwitch sw in listOfSwitches)
                                {

                                    listOfSwitchesObsrv.Add(sw);
                                    Console.WriteLine(sw);
                                }


                            }


                        });
                        
                        
                    }
                    Thread.Sleep(probe * 1000);
                }
            }


                       
        }

        // Get number of links, switches
        private void getSDNSummary()
        {
            String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/core/controller/summary/json";
            Console.WriteLine("URL=" + url);
            while (true)
            {
                using (var webClient = new System.Net.WebClient())
                {
                    var json = webClient.DownloadString(url);

                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    networkStatus = ser.Deserialize<NetworkStatus>(json);

                    Console.WriteLine(networkStatus);
                }

                Thread.Sleep(probe * 1000);
            }
            
        }

        // Get SDN controller status
        private void getSDNHealthly()
        {
            while(true)
            {
                using(var webClient = new System.Net.WebClient())
                {
                    String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/core/health/json";
                    Console.WriteLine("Healthy thread = " + url);
                    var json = webClient.DownloadString(url);
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    healthStatus = ser.Deserialize<HealthStatus>(json);
                    Console.WriteLine("Status=" + healthStatus);

                    Dispatcher.Invoke((Action)delegate ()
                    {
                        if (healthStatus.HealthyStatus)
                            isHealthBox.Text = "HEALTH";
                        else
                            isHealthBox.Text = "NOT";
                    });


                }

                Thread.Sleep(probe * 1000);
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

            System.Windows.Data.CollectionViewSource healthStatusViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("healthStatusViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // healthStatusViewSource.Source = [generic data source]
        }
    }
}
