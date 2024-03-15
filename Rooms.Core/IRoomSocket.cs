using System;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// Room Socket interface for specific implementations
    /// </summary>
    public interface IRoomSocket : IDisposable
    {

        /// <summary>
        /// Checks if the underlying Socket implementation is open.
        /// </summary>
        public bool IsAlive { get; }

        /// <summary>
        /// Send a message asynchronously. Close the underlying TCP Client if an invalid message is send.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns></returns>
        public Task SendAsync(RoomMessage message);

        /// <summary>
        /// Receive a message asynchronously. Close the underlying TCP Client if an invalid message is received.
        /// </summary>
        /// <returns>The message received.</returns>
        public Task<RoomMessage> ReceiveAsync();

    }
    
}