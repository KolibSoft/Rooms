using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{
    public class RoomNetworkStream : RoomStream
    {

        public NetworkStream Stream { get; private set; }

        protected override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token)
        {
            var result = await Stream.ReadAsync(buffer, token);
            return result;
        }

        protected override async ValueTask<int> WriteAsync(Memory<byte> buffer, CancellationToken token)
        {
            await Stream.WriteAsync(buffer, token);
            return buffer.Length;
        }

        public RoomNetworkStream(NetworkStream stream, ArraySegment<byte> buffer) : base(buffer) => Stream = stream;

    }
}