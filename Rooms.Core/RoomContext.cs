namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// Binds a socket with a message.
    /// </summary>
    public class RoomContext
    {

        /// <summary>
        /// Room socket.
        /// </summary>
        public IRoomSocket Socket { get; }

        /// <summary>
        /// Room message.
        /// </summary>
        public RoomMessage Message { get; }

        /// <summary>
        /// Constructs a new Room Context.
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