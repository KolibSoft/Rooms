using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{

    /// <summary>
    /// Binds a socket with a message.
    /// </summary>
    public readonly struct RoomContext
    {

        /// <summary>
        /// Binded socket.
        /// </summary>
        public IRoomSocket Socket { get; }

        /// <summary>
        /// Binded message.
        /// </summary>
        public RoomMessage Message { get; }

        /// <summary>
        /// Constructs a new context.
        /// </summary>
        /// <param name="socket">Socket to bind.</param>
        /// <param name="message">Message to bind.</param>
        public RoomContext(IRoomSocket socket, RoomMessage message)
        {
            Socket = socket;
            Message = message;
        }

    }

}