using System;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Protocol
{
    public static class RoomStreamUtils
    {
        public static async ValueTask ReadMessageAsync(this IRoomStream stream, RoomMessage message, CancellationToken token = default)
        {
            if (stream.IsDisposed) throw new ObjectDisposedException(nameof(IRoomStream));
            var protocol = new RoomProtocol();
            await stream.ReadProtocolAsync(protocol, token);
            message.Verb = protocol.Verb.ToString().Trim();
            message.Channel = (int)protocol.Channel;
            message.Content = protocol.Content.Data;
        }

        public static async ValueTask WriteMessageAsync(this IRoomStream stream, RoomMessage message, CancellationToken token = default)
        {
            if (stream.IsDisposed) throw new ObjectDisposedException(nameof(IRoomStream));
            var protocol = new RoomProtocol
            {
                Verb = RoomVerb.Parse($"{message.Verb.Trim()} "),
                Channel = (RoomChannel)message.Channel,
                Count = (RoomCount)message.Content.Length,
                Content = RoomContent.Create(message.Content)
            };
            await stream.WriteProtocolAsync(protocol, token);
        }
    }
}