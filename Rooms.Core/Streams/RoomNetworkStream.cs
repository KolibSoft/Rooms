using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Streams
{
    public class RoomNetworkStream : RoomStream
    {

        public TcpClient Client { get; private set; }
        public override bool IsAlive => !IsDisposed && Client.Connected;

        protected override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token)
        {
            if (!Client.Connected) return 0;
            var stream = Client.GetStream();
            var result = await stream.ReadAsync(buffer, token);
            return result;
        }

        protected override async ValueTask<int> WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token)
        {
            if (!Client.Connected) return 0;
            var stream = Client.GetStream();
            await stream.WriteAsync(buffer, token);
            return buffer.Length;
        }

        public RoomNetworkStream(TcpClient client, ArraySegment<byte> readBuffer, ArraySegment<byte> writeBuffer, RoomStreamOptions? options = null) : base(readBuffer, writeBuffer, options) => Client = client;
        public RoomNetworkStream(TcpClient client, int readBuffering = 1024, int writeBuffering = 1024, RoomStreamOptions? options = null) : this(client, new byte[readBuffering], new byte[writeBuffering], options) { }

    }
}
