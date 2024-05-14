using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Protocol
{
    public interface IRoomStream : IAsyncDisposable, IDisposable
    {
        public bool IsAlive { get; }
        public ValueTask ReadProtocolAsync(RoomProtocol protocol, CancellationToken token = default);
        public ValueTask ReadContentAsync(long count, Stream content, CancellationToken token = default);
        public ValueTask WriteProtocolAsync(RoomProtocol protocol, CancellationToken token = default);
        public ValueTask WriteContentAsync(long count, Stream content, CancellationToken token = default);
    }
}