using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Streams
{
    public class RoomWebStream : RoomStream
    {

        public WebSocket Socket { get; private set; }
        public override bool IsAlive => !IsDisposed && Socket.State == WebSocketState.Open;

        protected override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token)
        {
            if (Socket.State != WebSocketState.Open) return 0;
            var result = await Socket.ReceiveAsync(buffer, token);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, token);
                return 0;
            }
            if (result.MessageType == WebSocketMessageType.Text)
            {
                await Socket.CloseOutputAsync(WebSocketCloseStatus.InvalidMessageType, null, token);
                return 0;
            }
            return result.Count;
        }

        protected override async ValueTask<int> WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token)
        {
            if (Socket.State != WebSocketState.Open) return 0;
            await Socket.SendAsync(buffer, WebSocketMessageType.Binary, true, token);
            return buffer.Length;
        }


        public RoomWebStream(WebSocket socket, ArraySegment<byte> readBuffer, ArraySegment<byte> writeBuffer, RoomStreamOptions? options = null) : base(readBuffer, writeBuffer, options) => Socket = socket;
        public RoomWebStream(WebSocket socket, RoomStreamOptions? options = null) : base(options) => Socket = socket;

    }
}
