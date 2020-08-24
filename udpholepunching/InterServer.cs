using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

readonly public struct StateObject
{
    readonly public Socket socket;
    public const int BufferSize = 1024;
    readonly public byte[] buffer;
    readonly public StringBuilder sb;

    public StateObject(Socket socket)
    {
        this.socket = socket;
        this.buffer = new byte[BufferSize];
        this.sb = new StringBuilder();
    }
}

namespace udpholepunching
{
    class InterServer
    {
        ManualResetEvent allDone = new ManualResetEvent(false);

        void Listen(int port)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            Socket listener = new Socket(
                localEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();

                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(OnAccept, listener);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private static void OnAccept(IAsyncResult ar)
        {
            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);
            var state = new StateObject(handler);

            handler.BeginReceive(
                state.buffer,           // buffer
                0,                      // offset
                StateObject.BufferSize, // buffer size
                0,                      // flag
                OnRead,
                state
            );
        }

        private static void OnRead(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var socket = state.socket;

            var bytesRead = socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                var content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // all data read
                    Console.WriteLine("Read {0} bytes from socket \n Data:{1}", content.Length, content);
                    state.sb.Clear();

                    Send(socket, content);
                }
                else
                {
                    socket.BeginReceive(
                        state.buffer,
                        0,
                        StateObject.BufferSize,
                        0,
                        OnRead,
                        state
                    );
                }
            }
        }

        static void Send(Socket socket, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            socket.BeginSend(
                byteData,
                0,
                byteData.Length,
                0,
                OnSend,
                socket
            );
        }

        static void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;

                int bytesSent = socket.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client", bytesSent);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
