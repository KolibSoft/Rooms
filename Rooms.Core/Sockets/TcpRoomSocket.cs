using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{

    public class TcpRoomSocket : IRoomSocket
    {
        private bool disposed;

        public TcpClient Client { get; }

        public bool IsAlive => Client.Connected;

        public ArraySegment<byte> SendBuffer { get; private set; }
        public ArraySegment<byte> ReceiveBuffer { get; private set; }

        public async Task SendAsync(RoomMessage message)
        {
            message.CopyTo(SendBuffer);
            var stream = Client.GetStream();
            await stream.WriteAsync(SendBuffer[0..message.Length]);
        }

        public async Task ReceiveAsync(RoomMessage message)
        {
            var stream = Client.GetStream();
            var result = await stream.ReadAsync(ReceiveBuffer);
            message.CopyFrom(ReceiveBuffer[0..result]);
        }

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

        public TcpRoomSocket(TcpClient client, ArraySegment<byte> sendBuffer, ArraySegment<byte> receiveBuffer)
        {
            Client = client;
            SendBuffer = sendBuffer;
            ReceiveBuffer = receiveBuffer;
        }

        public TcpRoomSocket(TcpClient client, int sendBuffering = 1024, int receiveBuffering = 1024)
        {
            Client = client;
            SendBuffer = new byte[sendBuffering];
            ReceiveBuffer = new byte[receiveBuffering];
        }

    }

}