using DDOSDefender.JSONObj;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Xml;
using System.Numerics;
using Mahapps.JSONObj;

namespace Mahapps
{
    public partial class MainWindow
    {
        ObservableCollection<DDOSTable> DDosTable = new ObservableCollection<DDOSTable> ();

        // Load DDOS rules from xml file
        public void loadDDOSRules(String _filename)
        {
            if (_filename != null)
            {
                XmlDocument doc = new XmlDocument();
                if (File.Exists(_filename))
                {
                    try
                    {
                        doc.Load(_filename);
                        foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                        {
                            if (node.Name == "rule")
                            {
                                DDOSTable rule = new DDOSTable();
                                rule.ID = node.Attributes["id"].InnerText;
                                foreach (XmlNode ruleChild in node)
                                {
                                    if (ruleChild.Name == "switch")
                                    {
                                        rule.SwitchID = ruleChild["DPID"].InnerText;
                                        rule.Port = ruleChild["port"].InnerText;
                                    }
                                    else if (ruleChild.Name == "flow")
                                    {
                                        rule.IPDST = ruleChild["ipdst"].InnerText;
                                        rule.MAX_RX_BPS = ruleChild["maxRXBPS"].InnerText;
                                        rule.MAX_TX_BPS = ruleChild["maxTXBPS"].InnerText;

                                    }
                                    else if (ruleChild.Name == "action")
                                    {
                                        if (ruleChild.InnerText == "DROP")
                                        {
                                            rule.Action = DDOSTable.action.DROP;
                                        }
                                        else
                                        {
                                            rule.Action = DDOSTable.action.ALERT;
                                        }
                                    }

                                }
                                {

                                }
                                DDosTable.Add(rule);

                            }

                        }
                    }
                    catch (FileNotFoundException e)
                    {
                        MessageBox.Show("Cannot load DDOS config file. DDOS rules remain empty\n" + e.Message + "\n" + e.StackTrace, "Error 5", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                //DDosTable.Add(new DDOSTable { ID = 1, SwitchID = "0.....1" });
            }
        }

        // DDOS checker
        // MAIN ALGORITHM
        public void DDOSThread()
        {
            while (true)
            {
                Console.WriteLine("DEBUG DDOS CHECKER START");
                // Check each entry in DDOS Table
                foreach (DDOSTable _t in DDosTable)
                {
                    // Check each statistic
                    foreach (Statistics _s in stats)
                    {
                        // chech each switch@port
                        if(_t.SwitchID == _s.dpid && _t.Port == _s.port)
                        {
                            BigInteger tMAXRX = BigInteger.Parse(_t.MAX_RX_BPS);
                            BigInteger sMAXRX = BigInteger.Parse(_s.BPPSRX);
                            // DDOS DETECTED on RX direction
                            if (sMAXRX > tMAXRX)
                            {
                                // make an alert in event logs
                                if(_t.Action == DDOSTable.action.ALERT)
                                {
                                    String alarmMSG = "DDOS detected! " + _t.SwitchID + ": " + _t.Port +
                                        " \\current MAX RX BPS value is higher than expected. Current value is "
                                        + sMAXRX + " threshold is " + tMAXRX;
                                    Dispatcher.BeginInvoke(
                                        (Action)(
                                        () => eventList.Add(new EventItem(alarmMSG, EventItem.SEVERITY.Alert))
                                        )
                                        );
                                }
                                if (_t.Action == DDOSTable.action.DROP)
                                {
                                    Console.WriteLine(_t.SwitchID + ":" + _t.Port + ":" + _t.MAX_RX_BPS);
                                    // TO DO wyszukac w Flow Table pasujacy ruch.
                                    // pattern to 2 x time 
                                    
                                    foreach(SDNFlowTable.Flow flow in sdnFlowTable.flows)
                                    {
                                        if (int.Parse(flow.durationSeconds)  < (3*probe) && flow.match.in_port == _t.Port)
                                        {
                                            Console.WriteLine("Atak jest mniejszy niz 15 sekund");
                                            Console.WriteLine("Port {0} pasuje", _t.Port);
                                            
                                        }
                                    }
                                }
                                
                            }
                            
                        }
                    }
                    
                }

                Console.WriteLine("DEBUG DDOS CHECKER START");

                Thread.Sleep(probe * 1000);
            }

        }
    }

   
}
