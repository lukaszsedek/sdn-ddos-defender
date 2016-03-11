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

namespace Mahapps
{
    public partial class MainWindow
    {
        ObservableCollection<Statistics> stats = new ObservableCollection<Statistics>();

        public void getStatsThread()
        {
            while (true)
            {
                stats = new ObservableCollection<Statistics>();
                HashSet<String> tempSwList = new HashSet<string>();
                // 1. Znajdz unikalne switche
                foreach (DDOSTable dos in DDosTable)
                {
                    tempSwList.Add(dos.SwitchID);
                    
                }
                //2. Zbierz statystki dla nich
                foreach(String dpid in tempSwList)
                {
                    using (var webClient = new System.Net.WebClient())
                    {
                        //"wm/statistics/bandwidth/00:00:00:00:00:00:00:04/3/json"
                        String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/statistics/bandwidth/" + dpid + "/all/json";
                        //     Console.WriteLine(url);
                       
                        var json = webClient.DownloadString(url);
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        //         DataContractJsonSerializer
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ObservableCollection<Statistics>));
                        MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
                        stats = serializer.ReadObject(ms) as ObservableCollection<Statistics>; 

                        Dispatcher.BeginInvoke((Action)(() => statsGrid.ItemsSource = stats));
                        
                    }
                }
                
                // Dispatcher.BeginInvoke((Action)(() => stats.Add(new Statistics { dpid = "ad" })));
               
                Thread.Sleep(probe * 1000);
            }
        }
    }
}
