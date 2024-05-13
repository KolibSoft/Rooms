using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

/*
namespace KolibSoft.Rooms.Core.Sockets
{
    public class RoomTcpSocket : IRoomSocket
    {

        public TcpClient Client { get; private set; }
        public bool IsDisposed => _disposed;

        public async ValueTask ReceiveProtocolAsync(RoomProtocol protocol, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomTcpSocket));
            using var stream = new RoomNetworkStream(Client, _buffer);
            await stream.ReadProtocolAsync(protocol, token);
        }

        public async ValueTask SendProtocolAsync(RoomProtocol protocol, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomTcpSocket));
            using var stream = new RoomNetworkStream(Client, _buffer);
            await stream.WriteProtocolAsync(protocol, token);
        }

        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                _buffer = default;
                _disposed = true;
            }
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            _ = DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        public RoomTcpSocket(TcpClient client, ArraySegment<byte> buffer)
        {
            Client = client;
            _buffer = buffer;
        }

        public RoomTcpSocket(TcpClient client, int buffering = 1024) : this(client, new byte[buffering]) { }

        private ArraySegment<byte> _buffer;
        private bool _disposed = false;

    }
}
*/
