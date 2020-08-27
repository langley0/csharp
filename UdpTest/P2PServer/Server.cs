using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace P2PServer 
{
    public class ClientInfo 
    {
        public long Id { get; set; }
        public bool Authenticated { get; set; }
        public string Name {get; set; }

        public TcpClient Socket { get; set; }
        public IPEndPoint ExternalEndPoint { get; set;}

        public Packet ToPacket(Message msg) 
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, msg);
            var data = stream.ToArray();
            return new Packet(Packet.MagicNumber, ++this.sendSequence, msg.Type, data, (ushort)stream.Length);
        }

        ushort sendSequence = 0;
    }


    public class Server 
    {
        const int Port = 7777;
        static long NextId = 0;

        static Dictionary<long, ClientInfo> clients = new Dictionary<long, ClientInfo>();

        public static void Start()
        {
            var tcpThread = new Thread(new ThreadStart(TcpListen));
            tcpThread.Name = "TcpThread";
            tcpThread.Start();

            var udpThread = new Thread(new ThreadStart(UdpListen));
            udpThread.Name = "UdpThread";
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
                    var id = ++NextId;
                    var info = new ClientInfo();
                    info.Id = id;
                    info.Authenticated = false;
                    info.ExternalEndPoint = null;
                    info.Socket = client;
                    clients.Add(id, info);

                    var clientThread = new Thread(() => {
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
                            client.Close();
                        }
                    });
                    
                    clientThread.Start();

                } catch(Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        
        static void HandlePacket(long id, Packet packet) 
        {
            var info = clients[id];
            var formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(packet.Data, 0, packet.DataLength);

            switch(packet.MessageType) {
                case MessageType.AUTH_REQUEST: {
                    Debug.Assert(info.Authenticated == false)   ;
                    var msg = (AuthRequest)formatter.Deserialize(stream);
                    info.Authenticated = true;
                    info.Name = msg.Name;

                    var reply = new AuthReply() { Id = id, };
                    var sendPacket = info.ToPacket(reply);
                    info.Socket.GetStream().Write(sendPacket.ToBytes());

                    Console.WriteLine("Login {0}", info.Name);
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
                var packet = receivedBytes.ToPacket();
                var formatter = new BinaryFormatter();
                 MemoryStream stream = new MemoryStream(packet.Data, 0, packet.DataLength);


                switch(packet.MessageType) {
                    case MessageType.KEEP_ALIVE: {
                        var msg = (KeepAlive)formatter.Deserialize(stream);
                        // keep alive 등록을 한다
                        Console.WriteLine("KeepAlive from {0}", ipEndPoint);
                    }
                    break;
                }
            }
        }

    }
}