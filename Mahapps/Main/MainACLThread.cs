using DDOSDefender.JSONObj;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;

namespace Mahapps
{
    public partial class MainWindow
    {
        // Periodicaly gather ACL rules
        public void aclThreadFunction()
        {
            while (true)
            {   
                try
                {
                    // GET ACL
                    using (var webClient = new System.Net.WebClient())
                    {
                        String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/acl/rules/json";
                        var json = webClient.DownloadString(url);
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        aclList = ser.Deserialize<ObservableCollection<ACL>>(json);
                        aclGrid.Dispatcher.BeginInvoke((Action) delegate() {
                            aclGrid.ItemsSource = aclList;
                            ACLTile.Count = "" + aclList.Count;
                            }
                         );
                    }
                }
                catch (WebException e)
                {
                    MessageBox.Show(_settings.IpAddress + " address is unreachable\n" + e.StackTrace, "Error 4", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // Console.WriteLine("Flow table STOP");
                Thread.Sleep(probe * 1000);
            }
            
        } // aclThreadFunction

        private void ACLTile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    String url = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/acl/clear/json";
                    var json = webClient.DownloadString(url);
                }

            }
            catch (WebException exception)
            {
                MessageBox.Show(_settings.IpAddress + " address is unreachable\n" + exception.StackTrace, "Error 4", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /*
         * nw-proto	string	"TCP" or "UDP" or "ICMP" (ignoring case)
         * src-ip	IPv4 address[/mask]	Either src-ip or dst-ip must be specified.
         * dst-ip	IPv4 address[/mask]	Either src-ip or dst-ip must be specified.
         * tp-dst	number	Valid when nw-proto == "TCP" or "UDP".
         * action	string	"DENY" or "ALLOW" (ignoring case), set to "DENY" if not specified.
        */
        private void addNewACL(SDNFlowTable.Flow flow)
        {
            // Check TCP/UDP
            String nwProtocol = "";
            String srcIP = flow.match.ipv4_src;
            String dstIP = flow.match.ipv4_dst;
            String dstPort = "";
            if((flow.match.udp_dst == null || flow.match.udp_dst == "") && flow.match.tcp_dst != null){
                nwProtocol = "TCP";
                dstPort = flow.match.tcp_dst;
                Console.Write("Network Protocol found {0}", nwProtocol);
            }else if ( flow.match.udp_dst != null && (flow.match.tcp_dst == null || flow.match.tcp_dst == ""))
            {
                nwProtocol = "UDP";
                dstPort = flow.match.udp_dst;
                Console.Write("Network Protocol found {0}", nwProtocol);
            }
            else
            {
                Console.WriteLine("Network protocol unknkown {0}", flow.match.tcp_dst);
                return;
            }

            // IP addresses
            if (srcIP == null || dstIP == null)
                return;
            if (srcIP.Length < 7)
            {
                Console.WriteLine("IP addresses too short {0}\t{1}\n{2},{3}", srcIP, srcIP.Length, dstIP, dstIP.Length);
                return;
            }
            
            
            ACL aclToPush = new ACL();
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{ ");
            jsonBuilder.Append("\"nw-proto\":\"" + nwProtocol + "\", ");
            jsonBuilder.Append("\"src-ip\":\"" + srcIP + "/32\", ");
            jsonBuilder.Append("\"dst-ip\":\"" + dstIP + "/32\", ");
            jsonBuilder.Append("\"action\":\"" + "DENY" + "\", ");
            jsonBuilder.Append("\"tp-dst\":\"" + dstPort + "\" ");
            jsonBuilder.Append(" }");

            // Build POST HTTPP request
            String urlFirewall = "http://" + _settings.IpAddress + ":" + _settings.Port + "/wm/acl/rules/json";

           try
            {
                WebRequest request = WebRequest.Create(urlFirewall);
                request.Method = "POST";
                String postData = jsonBuilder.ToString();
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                addLogUI(((HttpWebResponse)response).StatusDescription + "New DENY rule addedd " + flow, 5);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
            } catch (WebException exception)
            {
                Console.WriteLine(exception.StackTrace);
                addLogUI(exception.StackTrace, 0);
            }
        
        }
        
    }
}
