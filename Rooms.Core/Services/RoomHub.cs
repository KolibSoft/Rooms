using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Streams;

namespace KolibSoft.Rooms.Core.Services
{
    public class RoomHub : RoomService
    {

        protected override ValueTask OnReceiveAsync(IRoomStream stream, RoomMessage message, CancellationToken token)
        {
            var channel = message.Channel;
            if (channel == 0) Enqueue(stream, message);
            else if (channel == -1)
            {
                var hash = stream.GetHashCode();
                foreach (var _stream in Streams)
                    if (_stream != stream)
                    {
                        message = new RoomMessage
                        {
                            Verb = message.Verb,
                            Channel = hash ^ _stream.GetHashCode(),
                            Content = message.Content
                        };
                        Enqueue(_stream, message);
                    }
            }
            else
            {
                var hash = stream.GetHashCode() ^ channel;
                var _stream = Streams.FirstOrDefault(x => x.GetHashCode() == hash);
                if (_stream != null) Enqueue(_stream, message);
            }
            return ValueTask.CompletedTask;
        }

        public RoomHub(RoomServiceOptions? options = null) : base(options) { }

    }
}