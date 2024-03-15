using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// A buffering tcp client to send and receive messages asynchronously.
    /// </summary>
    public class TcpRoomSocket : IRoomSocket
    {

        /// <summary>
        /// Disposed flag
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The underlying provided TCP Client.
        /// </summary>
        public TcpClient Client { get; }

        /// <summary>
        /// Checks if the underlying TCP Client is open.
        /// </summary>
        public bool IsAlive => Client.Connected;

        /// <summary>
        /// The underlying Send Buffer.
        /// </summary>
        public ArraySegment<byte> SendBuffer { get; private set; }

        /// <summary>
        /// The underlying Send Buffer.
        /// </summary>
        public ArraySegment<byte> ReceiveBuffer { get; private set; }

        /// <summary>
        /// Send a message asynchronously. Close the underlying TCP Client if an invalid message is send.
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
                Client.Close();
                throw new IOException("Message is too big");
            }
            if (!message.Validate())
            {
                Client.Close();
                throw new FormatException($"Invalid message format: {message}");
            }
            message.CopyTo(SendBuffer);
            var data = SendBuffer.Slice(0, message.Length);
            var stream = Client.GetStream();
            await stream.WriteAsync(data);
        }

        /// <summary>
        /// Receive a message asynchronously. Close the underlying TCP Client if an invalid message is received.
        /// </summary>
        /// <returns>The message received.</returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="FormatException"></exception>
        public async Task<RoomMessage> ReceiveAsync()
        {
            if (disposed) throw new ObjectDisposedException(null);
            var stream = Client.GetStream();
            var count = await stream.ReadAsync(ReceiveBuffer);
            if (stream.DataAvailable)
            {
                Client.Close();
                throw new IOException("Too big message received");
            }
            var data = ReceiveBuffer.Slice(0, count);
            var message = new RoomMessage(data);
            if (!message.Validate())
            {
                Client.Close();
                throw new FormatException($"Invalid message received: {message}");
            }
            return message;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing) Client.Dispose();
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
        /// <param name="socket">A connected TCP Client.</param>
        /// <param name="sendBuffer">Send buffer.</param>
        /// <param name="receiveBuffer">Receive buffer.</param>
        public TcpRoomSocket(TcpClient client, ArraySegment<byte> sendBuffer, ArraySegment<byte> receiveBuffer)
        {
            Client = client;
            SendBuffer = sendBuffer;
            ReceiveBuffer = receiveBuffer;
        }

        /// <summary>
        /// Creates a new Socket with the specified buffering size.
        /// </summary>
        /// <param name="socket">A connected Web Socket.</param>
        /// <param name="bufferingSize">Buffering size to send and receive messages.</param>
        public TcpRoomSocket(TcpClient client, int bufferingSize = 1024)
        {
            Client = client;
            SendBuffer = new byte[bufferingSize];
            ReceiveBuffer = new byte[bufferingSize];
        }

    }

}