using System;
using System.Diagnostics;

namespace UdpTest
{

    public class Packet 
    {
        const int HeaderLength = 8;
        const ushort MagicNumber = 0x1234;

        public ushort Magic { get; private set; }
        public ushort Sequence { get; private set; }
        public ushort MessageType { get; private set; }
        public ushort DataLength { get; private set; }
        public byte[] Data { get; private set; }

        public Packet(ushort magic, ushort seq, ushort msgType, byte[] data)
        {
            Debug.Assert(magic == MagicNumber, "wrong magic number");
            Debug.Assert(data.Length < ushort.MaxValue);
            
            this.Magic = magic;
            this.Sequence = seq;
            this.MessageType = msgType;
            this.DataLength = (ushort)data.Length;
            this.Data = data;
        }
        
        public static Packet TryParse(byte[] bytes, int offset, int length) {

            var available = length - offset;
            if (available < HeaderLength) { return null; }
        
            ushort magic = BitConverter.ToUInt16(bytes, offset);
            ushort seqeunce = BitConverter.ToUInt16(bytes, offset = offset + sizeof(ushort));
            ushort type = BitConverter.ToUInt16(bytes, offset = offset + sizeof(ushort));
            ushort dataLength = BitConverter.ToUInt16(bytes, offset = offset + sizeof(ushort));

            if (available < HeaderLength + dataLength) { return null; }

            byte[] data = new byte[dataLength];
            Array.Copy(bytes, offset, data, 0, dataLength);
            

            return new Packet(magic, seqeunce, type, data);
        }
    }


    public class Message 
    {
        public string From { get; private set; }
        public string To { get; private set; }
        public string Content { get; private set; }
        public long ID { get; set; }
        public long recipientID { get; set; }

        public Message(string from,  string to, string content) 
        {
            this.From = from;
            this.To = to;
            this.Content = content;
        }
    }
}