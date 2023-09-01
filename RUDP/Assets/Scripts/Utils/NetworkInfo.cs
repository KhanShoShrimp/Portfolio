using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Khansho
{
    public static class NetworkInfo
    {
        public static IPAddress LocalIPAddress { get; private set; }
        public static IPAddress ExternalIPAddress { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            InitLocalIPAddress();
            InitExternalIPAddress();
        }

        private static void InitLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            LocalIPAddress = host.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
        }

        private static void InitExternalIPAddress()
        {
            var client = new WebClient().DownloadString("http://ipinfo.io/ip");
            ExternalIPAddress = IPAddress.Parse(client);
        }
    }
}