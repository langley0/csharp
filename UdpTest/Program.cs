using System;
using System.Threading;

namespace UdpTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var serverThread = new Thread(() => { 
                P2PServer.Server.Start(); 
            });
            
            var clientThread = new Thread(() => { 
                var client = new P2PClient.Client();
                client.Name = "Alpha";
                client.ConnectTcp("localhost", 7777);
            });
            
            serverThread.Start();
            clientThread.Start();

            
            serverThread.Join();
            clientThread.Join();
        }
    }
}
