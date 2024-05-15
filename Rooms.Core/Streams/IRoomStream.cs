using System;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Streams
{
    public interface IRoomStream : IAsyncDisposable, IDisposable
    {
        public bool IsAlive { get; }
        public ValueTask<RoomMessage> ReadMessageAsync(CancellationToken token = default);
        public ValueTask WriteMessageAsync(RoomMessage message, CancellationToken token = default);
    }
}