using System;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{
    public interface IRoomSocket : IAsyncDisposable, IDisposable
    {
        public bool IsDisposed { get; }
        public ValueTask ReceiveProtocolAsync(RoomProtocol protocol, CancellationToken token = default);
        public ValueTask SendProtocolAsync(RoomProtocol protocol, CancellationToken token = default);
    }
}