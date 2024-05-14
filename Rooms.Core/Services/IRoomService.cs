using System;
using System.IO;
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
        public ValueTask SendAsync(RoomProtocol protocol, Stream content, CancellationToken token = default);
        public ValueTask StartAsync();
        public ValueTask StopAsync();
    }
}