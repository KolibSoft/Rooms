using System;

namespace KolibSoft.Rooms.Core.Protocol
{

    public class RoomMessage
    {
        public string Verb { get; set; } = string.Empty;
        public int Channel { get; set; } = 0;
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }

}