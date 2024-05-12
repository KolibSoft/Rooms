using System;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{
    public static class RoomSocketUtils
    {
        public static async ValueTask ReceiveMessageAsync(this IRoomSocket socket, RoomMessage message, CancellationToken token = default)
        {
            if (socket.IsDisposed) throw new ObjectDisposedException(nameof(IRoomStream));
            var protocol = new RoomProtocol();
            await socket.ReceiveProtocolAsync(protocol, token);
            message.Verb = protocol.Verb.ToString().Trim();
            message.Channel = (int)protocol.Channel;
            message.Content = protocol.Content.Data;
        }

        public static async ValueTask SendMessageAsync(this IRoomSocket socket, RoomMessage message, CancellationToken token = default)
        {
            if (socket.IsDisposed) throw new ObjectDisposedException(nameof(IRoomStream));
            var protocol = new RoomProtocol
            {
                Verb = RoomVerb.Parse($"{message.Verb.Trim()} "),
                Channel = (RoomChannel)message.Channel,
                Count = (RoomCount)message.Content.Length,
                Content = RoomContent.Create(message.Content)
            };
            await socket.SendProtocolAsync(protocol, token);
        }
    }
}