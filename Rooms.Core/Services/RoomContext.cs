using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{

    public readonly struct RoomContext
    {

        public IRoomSocket Socket { get; }
        public RoomMessage Message { get; }

        public RoomContext(IRoomSocket socket, RoomMessage message)
        {
            Socket = socket;
            Message = message;
        }

    }

}