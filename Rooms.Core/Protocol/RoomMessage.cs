using System.IO;

namespace KolibSoft.Rooms.Core.Protocol
{
    public sealed class RoomMessage
    {
        public string Verb { get; set; } = string.Empty;
        public int Channel { get; set; } = 0;
        public Stream Content { get; set; } = Stream.Null;
    }
}