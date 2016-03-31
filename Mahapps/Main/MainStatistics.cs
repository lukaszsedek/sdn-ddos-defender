using DDOSDefender.JSONObj;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using Mahapps.JSONObj;
using System.Net;

namespace Mahapps
{
    public partial class MainWindow
    {
        ObservableCollection<Statistics> stats = new ObservableCollection<Statistics>();
        // Get Statistics Thread
        public void getStatsThread()
        {
            while (true)
            {

                try
                {
                    // Get statistics
                    using (var webClient = new System.Net.WebClient())
                    {
                        String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/statistics/bandwidth/all/all/json";
                        var json = webClient.DownloadString(url);
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ObservableCollection<Statistics>));
                        MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
                        stats = serializer.ReadObject(ms) as ObservableCollection<Statistics>;
                        Dispatcher.BeginInvoke((Action)(() => statsGrid.ItemsSource = stats));
                    }
                }
                catch (WebException e)
                {

                }
                Thread.Sleep(probe * 1000);
            }

               
            }
        }
    }

