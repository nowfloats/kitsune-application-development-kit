using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KitsuneWebsiteCrawlerService.Helpers
{
    public class ServiceInformationHelper
    {
        private Process process = null;
        private IPHostEntry host = null;
        public ServiceInformationHelper()
        {
            try
            {
                process = Process.GetCurrentProcess();
                host = Dns.GetHostEntry(Dns.GetHostName());
            }
            catch (Exception ex)
            {

            }
        }

        public int GetProcessId()
        {
            try
            {
                return process.Id;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public string GetInstancePrivateIpAddress()
        {
            try
            {
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return String.Empty;
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

    }
}
