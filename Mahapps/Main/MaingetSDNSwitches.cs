using Mahapps.JSONObj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mahapps
{
    public partial class MainWindow
    {



        //Method for checking SDN heartbeat
        private void getSDNSwitches()
        {

            while (true)
            {
                using (var webClient = new System.Net.WebClient())
                {
                    String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/core/controller/switches/json";

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

    }


}
