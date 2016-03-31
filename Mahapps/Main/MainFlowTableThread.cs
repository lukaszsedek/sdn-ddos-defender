using DDOSDefender.JSONObj;
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

namespace Mahapps
{
    public partial class MainWindow
    {
        // ObservableCollection<Flow> FlowTable = new ObservableCollection<Flow>();
        SDNFlowTable sdnFlowTable = new SDNFlowTable();

        void FlowTableThread()
        {

            while (true)
            {
                // FlowTable = new ObservableCollection<Flow>();
                sdnFlowTable.flows = new ObservableCollection<SDNFlowTable.Flow>();
                foreach(SDNSwitch sw in listOfSwitches)
                {
                    // pobierz flow
                   
                    try
                    {
                        using (var webClient = new System.Net.WebClient())
                        {
                            String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/core/switch/" + sw.switchDPID +"/flow/json";
                            //Console.WriteLine("Flow table URL  " + url);
                            var json = webClient.DownloadString(url);
                            JavaScriptSerializer ser = new JavaScriptSerializer();
                            //Console.WriteLine("Flow table json  " + json);
                            SDNFlowTable tempTable = ser.Deserialize<SDNFlowTable>(json);
                            foreach (SDNFlowTable.Flow f in tempTable.flows)
                            {
                                f.switchID = sw.switchDPID;
                                Dispatcher.BeginInvoke((Action)(
                                    ()=> sdnFlowTable.flows.Add(f)
                                    ));
                                
                            }
                            

                            Dispatcher.BeginInvoke((Action)(
                                () => FlowTableGird.ItemsSource = sdnFlowTable.flows
                                ));
                            
                        }

                    }
                    catch(WebException e)
                    {
                        // TODO

                    }
                }
               // Console.WriteLine("Flow table STOP");
                Thread.Sleep(probe * 1000);
            }
            

        }
  
    }
}
