using System.IO;
using KolibSoft.Rooms.Core.Streams;

namespace KolibSoft.Rooms.Core.Services
{
    public sealed class RoomMessage
    {

        public IRoomStream Source { get; private set; }
        public string Verb { get; set; } = string.Empty;
        public int Channel { get; set; } = 0;
        public Stream Content { get; set; } = Stream.Null;

        public RoomMessage(IRoomStream source)
        {
            Source = source;
        }

    }
}