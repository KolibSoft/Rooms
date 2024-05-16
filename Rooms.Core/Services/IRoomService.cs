using System;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Streams;

namespace KolibSoft.Rooms.Core.Services
{
    public interface IRoomService : IAsyncDisposable, IDisposable
    {
        public bool IsRunning { get; }
        public ValueTask ListenAsync(IRoomStream stream, CancellationToken token = default);
        public ValueTask SendAsync(int index, RoomMessage message, CancellationToken token = default);
        public void Start();
        public void Stop();
    }
}