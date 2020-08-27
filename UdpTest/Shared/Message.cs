using System;
using System.Net;

namespace Shared 
{
    public class MessageType 
    {
        public const ushort KEEP_ALIVE = 1;
        public const ushort SYNC_REQUEST = 2;
        public const ushort SYNC_REPLY = 5;

        public const ushort AUTH_REQUEST = 3;
        public const ushort AUTH_REPLY = 4;
    }

    public interface Message
    {
        ushort Type { get; }
    }

    [Serializable]
    public class AuthRequest : Message {
        
        public ushort Type 
        { 
            get { return MessageType.AUTH_REQUEST; } 
        }

        public string Name {
            get; set;
        }

    }

    [Serializable]
    public class AuthReply : Message {
        
        public ushort Type 
        { 
            get { return MessageType.AUTH_REPLY; } 
        }

        public long Id { get; set; }
    }

    [Serializable]
    public class KeepAlive : Message {
        
        public ushort Type 
        { 
            get { return MessageType.KEEP_ALIVE; } 
        }

    }

    [Serializable]
    public class SyncRequest : Message {
        public ushort Type 
        {
            get { return MessageType.SYNC_REQUEST; }
        }

    }


    [Serializable]
    public class SyncReply : Message {
        public interface Client {
            long Id { get; }
            string Name { get; }
            IPEndPoint EndPoint { get; }
        }

        public ushort Type 
        {
            get { return MessageType.SYNC_REPLY; }
        }

        public Client[] Clients { get; set; }
    }
}