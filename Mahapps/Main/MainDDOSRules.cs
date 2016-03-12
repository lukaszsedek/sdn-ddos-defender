using DDOSDefender.JSONObj;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

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
        public void DDOSThread()
        {
            while (true)
            {
                Console.WriteLine("DEBUG DDOS CHECKER");

                Thread.Sleep(probe * 1000);
            }

        }
    }

   
}
