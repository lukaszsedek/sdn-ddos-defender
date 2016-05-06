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
            // Check null reference
            if (_filename == null)
            {
                addLogUI("DDOS rule xml file is empty", 3);
                return;
            }
            // Check filesystem
            if (!File.Exists(_filename))
            {
                addLogUI("Filename " + _filename + " does not exists", 3);
                return;
            }

            XmlDocument doc = new XmlDocument();
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

        // DDOS checker
        // MAIN ALGORITHM
        public void DDOSThread()
        {
            while (true)
            {
                // Check each entry in DDOS Table
                foreach (DDOSTable _t in DDosTable)
                {
                    // Check each statistic
                    foreach (Statistics _s in stats)
                    {
                        // check each switch@port
                        if(_t.SwitchID == _s.dpid && _t.Port == _s.port)
                        {
                            // TX
                            BigInteger tMAXTX = BigInteger.Parse(_t.MAX_TX_BPS);
                            BigInteger sMAXTX = BigInteger.Parse(_s.BPSTX);
                            // RX
                            BigInteger tMAXRX = BigInteger.Parse(_t.MAX_RX_BPS);
                            BigInteger sMAXRX = BigInteger.Parse(_s.BPPSRX);
                            // DDOS DETECTED on RX direction
                            if (sMAXRX > tMAXRX)
                            {
                                // make an alert in event logs
                                if(_t.Action == DDOSTable.action.ALERT)
                                {
                                    String alarmMSG = "DDOS detected! " + _t.SwitchID + ": " + _t.Port +
                                        " current MAX RX BPS value is higher than expected. Current value is "
                                        + sMAXRX + " threshold is " + tMAXRX;
                                    addLogUI(alarmMSG, 1);
                                }
                                // block suspected flows
                                if (_t.Action == DDOSTable.action.DROP)
                                {
                                    
                                    foreach(SDNFlowTable.Flow flow in sdnFlowTable.flows)
                                    {
                                        // TO DO !!! Sprawdzanie tylko portu
                                        if (int.Parse(flow.durationSeconds)  < (3*probe) && flow.match.in_port == _t.Port && flow.match.ipv4_dst == _t.IPDST)
                                        {
                                            Console.WriteLine("Switch {0} Port {1} flow duration {2} flow match {3}",_t.SwitchID, _t.Port, flow.durationSeconds, flow.match);
                                            //            applyBlockFirewallRule(flow);
                                           
                                            addNewACL(flow);
                                        }
                                    }
                                }
                                
                            }
                            // DDOS DETECTED on TX direction
                            if ( sMAXTX > tMAXRX )
                            {
                                // make an alert in event logs
                                if(_t.Action == DDOSTable.action.ALERT)
                                {
                                    String alarmMSG = "DDOS detected! " + _t.SwitchID + ": " + _t.Port +
                                        " current MAX TX BPS value is higher than expected. Current value is "
                                        + sMAXTX + " threshold is " + tMAXTX;
                                    addLogUI(alarmMSG, 1);
                                }
                                if (_t.Action == DDOSTable.action.DROP)
                                {
                                    foreach (SDNFlowTable.Flow flow in sdnFlowTable.flows)
                                    {
                                        if (int.Parse(flow.durationSeconds) < (3 * probe) && flow.match.in_port == _t.Port)
                                        {
                                            Console.WriteLine("Switch {0} Port {1} flow duration {2} flow match {3}", _t.SwitchID, _t.Port, flow.durationSeconds, flow.match);
                                            //            applyBlockFirewallRule(flow);

                                            addNewACL(flow);

                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                    
                }
                Thread.Sleep(probe * 1000);
            }

        }

        public void applyBlockFirewallRule(SDNFlowTable.Flow flow)
        {
            String sourceIPv4 = flow.match.ipv4_src;
            String destinationIPv4 = flow.match.ipv4_dst;
            if(sourceIPv4 == null)
            {
                sourceIPv4 = "";
            }
            if( destinationIPv4 == null)
            {
                destinationIPv4 = "";
            }
            String transportProtocol = "";
            String tp_src = "";
            String tp_dst = "";
            if (flow.match.tcp_dst != "" && flow.match.udp_dst == null)
            {
                transportProtocol = "TCP";
                tp_src = flow.match.tcp_src;
                tp_dst = flow.match.tcp_dst;
            }
            else if (flow.match.udp_dst != "")
            {
                transportProtocol = "UDP";
                tp_dst = flow.match.udp_dst;
                tp_src = flow.match.udp_src;
            }

            Console.Write("Creaing rule \n IP SRC {0}\n IP DST {1}\n NW-PROTO {2}\n, T SRC {3}\n T DST{4}\n", sourceIPv4, destinationIPv4, transportProtocol, tp_src, tp_dst);

            if (sourceIPv4.Length > 7 && destinationIPv4.Length > 7)
            {
                addCustomFirewallRule(flow.switchID, "DENY", sourceIPv4, destinationIPv4, transportProtocol, tp_src, tp_dst);
            }
            else
                Console.WriteLine("Wrong flow");
        }
    }

   
}
