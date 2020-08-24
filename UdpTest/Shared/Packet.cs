using System;
using System.Diagnostics;
using System.IO;

namespace Shared
{

    public class Packet 
    {
        const int HeaderLength = 8;
        public const ushort MagicNumber = 0x1234;

        public ushort Magic { get; private set; }
        public ushort Sequence { get; private set; }
        public ushort MessageType { get; private set; }
        public ushort DataLength { get; private set; }
        public byte[] Data { get; private set; }

        public Packet(ushort magic, ushort seq, ushort msgType, byte[] data, int length)
        {
            Debug.Assert(magic == MagicNumber, "wrong magic number");
            Debug.Assert(data.Length < ushort.MaxValue);
            
            this.Magic = magic;
            this.Sequence = seq;
            this.MessageType = msgType;
            this.DataLength = (ushort)length;
            this.Data = data;
        }

        public static Packet TryParse(byte[] bytes, int offset, int length) {

            var available = length - offset;
            if (available < HeaderLength) { return null; }
        
            ushort magic = BitConverter.ToUInt16(bytes, offset);
            ushort seqeunce = BitConverter.ToUInt16(bytes, offset = offset + sizeof(ushort));
            ushort type = BitConverter.ToUInt16(bytes, offset = offset + sizeof(ushort));
            ushort dataLength = BitConverter.ToUInt16(bytes, offset = offset + sizeof(ushort));
            offset = offset + sizeof(ushort);

            if (available < HeaderLength + dataLength) { return null; }

            byte[] data = new byte[dataLength];
            Array.Copy(bytes, offset, data, 0, dataLength);
            

            return new Packet(magic, seqeunce, type, data, dataLength);
        }

        public byte[] ToBytes() 
        {
            MemoryStream stream = new MemoryStream(HeaderLength + this.DataLength);
            stream.Write(BitConverter.GetBytes(this.Magic));
            stream.Write(BitConverter.GetBytes(this.Sequence));
            stream.Write(BitConverter.GetBytes(this.MessageType));
            stream.Write(BitConverter.GetBytes(this.DataLength));
            stream.Write(this.Data, 0, this.DataLength);
            return stream.ToArray();
        }
    }
}