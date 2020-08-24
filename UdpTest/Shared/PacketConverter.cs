using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Shared
{
    public static class PacketConverter 
    {
        public static Packet ToPacket(this byte[] bytes)
        {
            return Packet.TryParse(bytes, 0, bytes.Length);
        }
    }
}