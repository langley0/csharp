using System;
using System.Net;
using System.Net.Sockets;

namespace UdpTest
{
    public class ClientInfo
    {
        public TcpClient  Client { get; set; }
        public IPEndPoint ExternalEndpoint { get; set; }
        public IPEndPoint InternalEndpoint { get; set; }

        
    }
}