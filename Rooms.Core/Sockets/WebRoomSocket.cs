using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{

    /// <summary>
    /// Web Socket implementation of a Room socket.
    /// </summary>
    public class WebRoomSocket : IRoomSocket
    {

        /// <summary>
        /// Dispose flag.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Underlying Web Socket.
        /// </summary>
        public WebSocket Socket { get; }

        /// <summary>
        /// Checks if the Web Socket still open.
        /// </summary>
        public bool IsAlive => Socket.State == WebSocketState.Open;

        /// <summary>
        /// Buffer to store send data.
        /// </summary>
        public ArraySegment<byte> SendBuffer { get; private set; }

        /// <summary>
        /// Buffer to store receive data.
        /// </summary>
        public ArraySegment<byte> ReceiveBuffer { get; private set; }

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">If the socket was disposed.</exception>
        public async Task SendAsync(RoomMessage message)
        {
            if (disposed) throw new ObjectDisposedException(null);
            if (!message.Validate())
                throw new FormatException($"Invalid message format: {message}");
            message.CopyTo(SendBuffer);
            await Socket.SendAsync(SendBuffer[0..message.Length], WebSocketMessageType.Binary, true, default);
        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <param name="message">Message to receive.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">If the socket was disposed.</exception>
        public async Task ReceiveAsync(RoomMessage message)
        {
            if (disposed) throw new ObjectDisposedException(null);
            var result = await Socket.ReceiveAsync(ReceiveBuffer, default);
            message.CopyFrom(ReceiveBuffer[0..result.Count]);
            if (!message.Validate())
                throw new FormatException($"Invalid message format: {message}");
        }

        /// <summary>
        /// Internal dispose implementation.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing) Socket.Dispose();
                SendBuffer = default;
                ReceiveBuffer = default;
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Constructs a new Room socket around a Web Socket.
        /// </summary>
        /// <param name="socket">Opened Web socket.</param>
        /// <param name="sendBuffer">Buffer to store send data.</param>
        /// <param name="receiveBuffer">Buffer to store receive data.</param>
        public WebRoomSocket(WebSocket socket, ArraySegment<byte> sendBuffer, ArraySegment<byte> receiveBuffer)
        {
            Socket = socket;
            SendBuffer = sendBuffer;
            ReceiveBuffer = receiveBuffer;
        }

        /// <summary>
        /// Constructs a new Room socket around a Web Socket.
        /// </summary>
        /// <param name="client">Opened Web Socket.</param>
        /// <param name="sendBuffering">Send buffer size.</param>
        /// <param name="receiveBuffering">Receive buffer size.</param>
        public WebRoomSocket(WebSocket socket, int sendBuffering = 1024, int receiveBuffering = 1024)
        {
            Socket = socket;
            SendBuffer = new byte[sendBuffering];
            ReceiveBuffer = new byte[receiveBuffering];
        }

        public const string SubProtocol = "Room";

    }

}