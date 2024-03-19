using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// A buffering web socket to send and receive messages asynchronously.
    /// </summary>
    public class WebRoomSocket : IRoomSocket
    {

        /// <summary>
        /// Disposed flag
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The underlying provided Web Socket.
        /// </summary>
        public WebSocket Socket { get; }

        /// <summary>
        /// Checks if the underlying Web Socket is open.
        /// </summary>
        public bool IsAlive => Socket.State == WebSocketState.Open;

        /// <summary>
        /// The underlying Send Buffer.
        /// </summary>
        public ArraySegment<byte> SendBuffer { get; private set; }

        /// <summary>
        /// The underlying Receive Buffer.
        /// </summary>
        public ArraySegment<byte> ReceiveBuffer { get; private set; }

        /// <summary>
        /// Send a message asynchronously. Close the underlying Web Socket if an invalid message is send.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="FormatException"></exception>
        public async Task SendAsync(RoomMessage message)
        {
            if (disposed) throw new ObjectDisposedException(null);
            if (message.Length > SendBuffer.Count)
            {
                await Socket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError, null, default);
                throw new IOException("Message is too big");
            }
            if (!message.Validate())
            {
                await Socket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError, null, default);
                throw new FormatException($"Invalid message format: {message}");
            }
            message.CopyTo(SendBuffer);
            var data = SendBuffer.Slice(0, message.Length);
            await Socket.SendAsync(data, WebSocketMessageType.Text, true, default);
        }

        /// <summary>
        /// Receive a message asynchronously. Close the underlying Web Socket if an invalid message is received.
        /// </summary>
        /// <returns>The message received.</returns>
        /// <exception cref="IOException"></exception>
        public async Task<RoomMessage> ReceiveAsync()
        {
            if (disposed) throw new ObjectDisposedException(null);
            var result = await Socket.ReceiveAsync(ReceiveBuffer, default);
            var data = ReceiveBuffer.Slice(0, result.Count);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, default);
                throw new IOException("Web Socket closed");
            }
            if (!result.EndOfMessage)
            {
                await Socket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError, null, default);
                throw new IOException("Too big message received");
            }
            var message = new RoomMessage(data.ToArray());
            if (!message.Validate())
            {
                await Socket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError, null, default);
                throw new FormatException($"Invalid message received: {message}");
            }
            return message;
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
                ReceiveBuffer = null!;
                SendBuffer = null!;
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a new Socket with the specified buffers.
        /// </summary>
        /// <param name="socket">A connected Web Socket.</param>
        /// <param name="sendBuffer">Send buffer.</param>
        /// <param name="receiveBuffer">Receive buffer.</param>
        public WebRoomSocket(WebSocket socket, ArraySegment<byte> sendBuffer, ArraySegment<byte> receiveBuffer)
        {
            Socket = socket;
            SendBuffer = sendBuffer;
            ReceiveBuffer = receiveBuffer;
        }

        /// <summary>
        /// Creates a new Socket with the specified buffering size.
        /// </summary>
        /// <param name="socket">A connected Web Socket.</param>
        /// <param name="bufferingSize">Buffering size to send and receive messages.</param>
        public WebRoomSocket(WebSocket socket, int bufferingSize = 1024)
        {
            Socket = socket;
            SendBuffer = new byte[bufferingSize];
            ReceiveBuffer = new byte[bufferingSize];
        }

        /// <summary>
        /// Room protocol name.
        /// </summary>
        public const string SubProtocol = "Room";

    }

}