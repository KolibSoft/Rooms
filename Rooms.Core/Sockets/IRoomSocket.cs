using System;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{

    /// <summary>
    /// Disposable Room socket interface.
    /// </summary>
    public interface IRoomSocket : IDisposable
    {

        /// <summary>
        /// Checks if the socket can send/receive messages.
        /// </summary>
        public bool IsAlive { get; }

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        public Task SendAsync(RoomMessage message);

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <param name="message">Message to receive.</param>
        /// <returns></returns>
        public Task ReceiveAsync(RoomMessage message);

    }

}