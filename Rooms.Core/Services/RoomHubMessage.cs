using System.IO;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Streams;

namespace KolibSoft.Rooms.Core.Services
{
    public sealed class RoomHubMessage
    {

        public IRoomStream Source { get; private set; }
        public RoomProtocol Protocol { get; private set; }
        public Stream Content { get; private set; }

        public RoomHubMessage(IRoomStream source, RoomProtocol protocol, Stream content)
        {
            Source = source;
            Protocol = protocol;
            Content = content;
        }

    }
}