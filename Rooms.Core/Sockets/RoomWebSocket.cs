using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

/*
namespace KolibSoft.Rooms.Core.Sockets
{
    public class RoomWebSocket : IRoomSocket
    {

        public WebSocket Socket { get; private set; }
        public bool IsDisposed => _disposed;

        public async ValueTask ReceiveProtocolAsync(RoomProtocol protocol, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomWebSocket));
            using var stream = new RoomWebStream(Socket, _buffer);
            await stream.ReadProtocolAsync(protocol, token);
        }

        public async ValueTask SendProtocolAsync(RoomProtocol protocol, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomWebSocket));
            using var stream = new RoomWebStream(Socket, _buffer);
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

        public RoomWebSocket(WebSocket socket, ArraySegment<byte> buffer)
        {
            Socket = socket;
            _buffer = buffer;
        }

        public RoomWebSocket(WebSocket socket, int buffering = 1024) : this(socket, new byte[buffering]) { }

        private ArraySegment<byte> _buffer;
        private bool _disposed = false;

    }
}
*/
