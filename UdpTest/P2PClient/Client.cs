using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

namespace P2PClient 
{
    public class Client 
    {
        TcpClient socket;
        IPEndPoint serverEP;
        UdpClient udp;

        ushort tcpSequence;

        public string Name { get; set; }

        public void ConnectTcp(string host, int port)
        {
            this.socket = new TcpClient();
            this.tcpSequence = 0;
            var hostEntry = Dns.GetHostEntry(host);
            var ipV4Addr = hostEntry.AddressList.First((IPAddress addr) => { 
                return addr.AddressFamily == AddressFamily.InterNetwork;
            });
            this.serverEP = new IPEndPoint(ipV4Addr, port);
            this.socket.Connect(this.serverEP);

            Thread keepAlive = new Thread(new ThreadStart(() => {
                while(this.socket.Connected)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Client send KeepAlive to Server");
                    this.SendUdpMessage(new KeepAlive());
                }
            }));


            // 처음에 싱크를 맞춘다
            // 자신의 ID 를 서버에 보낸다. 이 명령은 로그인을 대신한다
            this.SendTcpMessage(new AuthRequest() { Name = Name});
            byte[] buffer = new byte[4096];
            while(this.socket.Connected) {
                var received = this.socket.GetStream().Read(buffer, 0, buffer.Length);
                // check disconnected
                if (received == 0) { return; }

                var packet = buffer.ToPacket();
                switch(packet.MessageType) {
                    case MessageType.AUTH_REPLY: {
                        // 인증이 완료되었다
                        // udp 연결을 시도한다
                        udp = new UdpClient();
                        keepAlive.Start();
                    }
                    break;
                }
            }
        }
        

        void ConnectUdp(string host, int port) 
        {

        }
        

        public void SendTcpMessage(Message msg)
        {
            var formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, msg);
            var data = stream.ToArray();
            this.tcpSequence += 1;
            var packet = new Packet(Packet.MagicNumber, this.tcpSequence, msg.Type, data, (int)stream.Length);
            this.socket.GetStream().Write(packet.ToBytes());
        }

        public void SendUdpMessage(Message msg) 
        {
            var formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, msg);
            var data = stream.ToArray();
            this.tcpSequence += 1;
            var packet = new Packet(Packet.MagicNumber, this.tcpSequence, msg.Type, data, (int)stream.Length);
            var bytes = packet.ToBytes();
            this.udp.Send(bytes, bytes.Length, this.serverEP);
            
        }
    }
}