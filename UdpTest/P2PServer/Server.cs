using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace P2PServer 
{
    public class Server 
    {
        const int Port = 7777;
        static long NextId = 0;

        public static void Start()
        {
            var tcpThread = new Thread(new ThreadStart(TcpListen));
            var udpThread = new Thread(new ThreadStart(UdpListen));
            tcpThread.Start();
            udpThread.Start();

            while(true)
            {
                Console.WriteLine("Type 'exit' to shutdown");
                if (Console.ReadLine().ToLower() == "exit")
                {
                    Console.WriteLine("Shutting down...");
                    Environment.Exit(0);
                }
            }
        }

        static void TcpListen() 
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
            var tcp = new TcpListener(ipEndPoint);
            tcp.Start();

            Console.WriteLine("Tcp Server started at port {0}", Port);

            while(true) 
            {
                try 
                {
                    var client = tcp.AcceptTcpClient();
                    var clientThread = new Thread(new ThreadStart(delegate () {
                        var id = ++NextId;
                        try 
                        {
                            while(client.Connected) 
                            {

                                var data = new byte[4096];
                                var bytesRead = client.GetStream().Read(data, 0 , data.Length);
                                if (bytesRead == 0) { break; }
                                if (client.Connected) 
                                {
                                    // process message
                                    var packet = data.ToPacket();
                                    HandlePacket(id, packet);
                                }
                            }
                        }
                        catch 
                        {
                            
                        }
                        finally 
                        {
                            // disconnect
                        }
                    }));
                    
                    clientThread.Start();

                } catch(Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        
        static void HandlePacket(long id, Packet packet) 
        {
            var formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(packet.Data, 0, packet.DataLength);

            switch(packet.MessageType) {
                case MessageType.KEEP_ALIVE: {
                   var msg = (KeepAlive)formatter.Deserialize(stream);
                   // keep alive 등록을 한다
                   Console.WriteLine("KeepAlive from {0}", id);
                }
                break;
            }
        }

        static void Disconnect(TcpClient client)
        {
            
        }

        static void UdpListen() 
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
            var udp = new UdpClient(Port);
            Console.WriteLine("Udp server started at port {0}", Port);

            while(true) 
            {
                var receivedBytes = udp.Receive(ref ipEndPoint);
                
                // data 를 처리한다
            }
        }

    }
}