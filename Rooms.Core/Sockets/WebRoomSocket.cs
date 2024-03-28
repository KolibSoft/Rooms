using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{

    public class WebRoomSocket : IRoomSocket
    {
        private bool disposed;

        public WebSocket Socket { get; }

        public bool IsAlive => Socket.State == WebSocketState.Open;

        public ArraySegment<byte> SendBuffer { get; private set; }
        public ArraySegment<byte> ReceiveBuffer { get; private set; }

        public async Task SendAsync(RoomMessage message)
        {
            if (disposed) throw new ObjectDisposedException(null);
            message.CopyTo(SendBuffer);
            await Socket.SendAsync(SendBuffer[0..message.Length], WebSocketMessageType.Binary, true, default);
        }

        public async Task ReceiveAsync(RoomMessage message)
        {
            if (disposed) throw new ObjectDisposedException(null);
            var result = await Socket.ReceiveAsync(ReceiveBuffer, default);
            message.CopyFrom(ReceiveBuffer[0..result.Count]);
        }

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

        public WebRoomSocket(WebSocket socket, ArraySegment<byte> sendBuffer, ArraySegment<byte> receiveBuffer)
        {
            Socket = socket;
            SendBuffer = sendBuffer;
            ReceiveBuffer = receiveBuffer;
        }

        public WebRoomSocket(WebSocket socket, int sendBuffering = 1024, int receiveBuffering = 1024)
        {
            Socket = socket;
            SendBuffer = new byte[sendBuffering];
            ReceiveBuffer = new byte[receiveBuffering];
        }

        public const string SubProtocol = "Room";

    }

}