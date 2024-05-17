using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Streams;

namespace KolibSoft.Rooms.Core.Services
{
    public class RoomHub : RoomService
    {

        protected IEnumerable<IRoomStream> Streams => _streams;

        protected override ValueTask OnReceiveAsync(IRoomStream stream, RoomMessage message, CancellationToken token)
        {
            var channel = message.Channel;
            if (channel == 0) Enqueue(stream, message);
            else if (channel == -1)
            {
                var hash = stream.GetHashCode();
                foreach (var _stream in _streams)
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
                var _stream = _streams.FirstOrDefault(x => x.GetHashCode() == hash);
                if (_stream != null) Enqueue(_stream, message);
            }
            return ValueTask.CompletedTask;
        }

        public override async ValueTask ListenAsync(IRoomStream stream, CancellationToken token = default)
        {
            if (_streams.Contains(stream)) throw new InvalidOperationException("Stream already listening");
            _streams = _streams.Add(stream);
            await base.ListenAsync(stream, token);
            _streams = _streams.Remove(stream);
        }

        public override void Enqueue(IRoomStream stream, RoomMessage message)
        {
            if (!_streams.Contains(stream)) throw new InvalidOperationException("Stream not listening");
            base.Enqueue(stream, message);
        }

        public void Send(RoomMessage message)
        {
            if (message.Channel == -1)
            {
                message.Channel = 0;
                foreach (var stream in Streams)
                    Enqueue(stream, message);
            }
            else
            {
                var target = Streams.FirstOrDefault(x => x.GetHashCode() == message.Channel);
                if (target != null)
                {
                    message.Channel = 0;
                    Enqueue(target, message);
                }
            }
        }

        public RoomHub(RoomServiceOptions? options = null) : base(options) { }

        private ImmutableArray<IRoomStream> _streams = ImmutableArray.Create<IRoomStream>();

    }
}