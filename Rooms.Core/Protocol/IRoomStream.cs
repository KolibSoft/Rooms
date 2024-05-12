using System;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Protocol
{
    public interface IRoomStream : IAsyncDisposable, IDisposable
    {
        public bool IsDisposed { get; }
        public ValueTask ReadProtocolAsync(RoomProtocol protocol, CancellationToken token = default);
        public ValueTask WriteProtocolAsync(RoomProtocol protocol, CancellationToken token = default);
    }
}