using System;

namespace Shared 
{
    public class MessageType 
    {
        public const ushort KEEP_ALIVE = 1;
        public const ushort SYNC_REQUEST = 2;


    }

    public interface Message
    {
        ushort Type { get; }
    }

    [Serializable]
    public class KeepAlive : Message {
        
        public ushort Type 
        { 
            get { return 1; } 
        }

    }

    [Serializable]
    public class SyncRequest : Message {
        [NonSerialized]
        public string Id;

        public ushort Type 
        {
            get { return 2; }
        }

    }
}