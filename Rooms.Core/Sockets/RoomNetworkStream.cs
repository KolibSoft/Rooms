using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{
    public class RoomNetworkStream : RoomStream
    {

        public TcpClient Client { get; private set; }

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

        public RoomNetworkStream(TcpClient client, ArraySegment<byte> readBuffer, ArraySegment<byte> writeBuffer) : base(readBuffer, writeBuffer) => Client = client;
        public RoomNetworkStream(TcpClient client, int readBuffering = 1024, int writeBuffering = 1024) : this(client, new byte[readBuffering], new byte[writeBuffering]) { }

    }
}
