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

        ushort tcpSequence;

        public void Connect(string host, int port)
        {
            this.socket = new TcpClient();
            this.tcpSequence = 0;
            var hostEntry = Dns.GetHostEntry(host);
            var ipV4Addr = hostEntry.AddressList.First((IPAddress addr) => { 
                return addr.AddressFamily == AddressFamily.InterNetwork;
            });
            this.serverEP = new IPEndPoint(ipV4Addr, port);
            this.socket.Connect(this.serverEP);

            // 처음에 싱크를 맞춘다
            // 자신의 ID 를 서버에 보낸다. 이 명령은 로그인을 대신한다

            Thread keepAlive = new Thread(new ThreadStart(() => {
                while(this.socket.Connected)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Client send KeepAlive to Server");
                    this.SendTcpMessage(new KeepAlive());
                }
            }));

            keepAlive.Start();
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
    }
}