using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{

    /// <summary>
    /// TCP implementation of a Room socket.
    /// </summary>
    public class TcpRoomSocket : IRoomSocket
    {

        /// <summary>
        /// Dispose flag.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Underlying TCP client.
        /// </summary>
        public TcpClient Client { get; }

        /// <summary>
        /// Checks if the TCP client still conncted.
        /// </summary>
        public bool IsAlive => Client.Connected;

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
            var stream = Client.GetStream();
            await stream.WriteAsync(SendBuffer[0..message.Length]);
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
            var stream = Client.GetStream();
            var result = await stream.ReadAsync(ReceiveBuffer);
            message.CopyFrom(ReceiveBuffer[0..result]);
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
                if (disposing) Client.Dispose();
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
        /// Constructs a new Room socket around a TCP client.
        /// </summary>
        /// <param name="client">Connected TCP client.</param>
        /// <param name="sendBuffer">Buffer to store send data.</param>
        /// <param name="receiveBuffer">Buffer to store receive data.</param>
        public TcpRoomSocket(TcpClient client, ArraySegment<byte> sendBuffer, ArraySegment<byte> receiveBuffer)
        {
            Client = client;
            SendBuffer = sendBuffer;
            ReceiveBuffer = receiveBuffer;
        }

        /// <summary>
        /// Constructs a new Room socket around a TCP client.
        /// </summary>
        /// <param name="client">Connected TCP client.</param>
        /// <param name="sendBuffering">Send buffer size.</param>
        /// <param name="receiveBuffering">Receive buffer size.</param>
        public TcpRoomSocket(TcpClient client, int sendBuffering = 1024, int receiveBuffering = 1024)
        {
            Client = client;
            SendBuffer = new byte[sendBuffering];
            ReceiveBuffer = new byte[receiveBuffering];
        }

    }

}