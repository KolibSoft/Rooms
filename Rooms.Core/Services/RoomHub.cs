using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Streams;

namespace KolibSoft.Rooms.Core.Services
{
    public class RoomHub : RoomService
    {

        public event EventHandler<RoomMessage>? MessageReceived;

        protected override void OnMessageReceived(IRoomStream stream, RoomProtocol protocol, Stream content)
        {
            _messages = _messages.Enqueue(new RoomMessage(stream)
            {
                Verb = protocol.Verb.ToString().Trim(),
                Channel = (int)protocol.Channel,
                Content = content
            });
        }

        protected override void OnMessageSent(IRoomStream stream, RoomProtocol protocol, Stream content) { }

        private async void TransmitAsync()
        {
            while (IsRunning)
            {
                if (_messages.Any())
                {
                    _messages = _messages.Dequeue(out RoomMessage message);
                    if (message.Channel == 0) MessageReceived?.Invoke(this, message);
                    else if (message.Channel == -1)
                    {
                        var hash = message.Source.GetHashCode();
                        foreach (var stream in Streams)
                            if (stream != message.Source)
                            {
                                var protocol = new RoomProtocol
                                {
                                    Verb = RoomVerb.Parse($"{message.Verb} "),
                                    Channel = (RoomChannel)(hash ^ stream.GetHashCode()),
                                    Count = (RoomCount)message.Content.Length
                                };
                                try
                                {
                                    await stream.WriteProtocolAsync(protocol);
                                    await stream.WriteContentAsync(message.Content.Length, message.Content);
                                }
                                catch (Exception error)
                                {
                                    if (Logger != null) await Logger.WriteLineAsync($"Error trasnmiting message: {error}");
                                }
                            }
                    }
                    else
                    {
                        var hash = message.Source.GetHashCode() ^ message.Channel;
                        var target = Streams.FirstOrDefault(x => x.GetHashCode() == hash);
                        if (target != null)
                        {
                            var protocol = new RoomProtocol
                            {
                                Verb = RoomVerb.Parse($"{message.Verb} "),
                                Channel = (RoomChannel)message.Channel,
                                Count = (RoomCount)message.Content.Length
                            };
                            try
                            {
                                await target.WriteProtocolAsync(protocol);
                                await target.WriteContentAsync(message.Content.Length, message.Content);
                            }
                            catch (Exception error)
                            {
                                if (Logger != null) await Logger.WriteLineAsync($"Error trasnmiting message: {error}");
                            }
                        }
                    }
                }
            }
        }

        protected override void OnStart() => TransmitAsync();

        public RoomHub(RoomServiceOptions? options = null) : base(options) { }

        private ImmutableQueue<RoomMessage> _messages = ImmutableQueue.Create<RoomMessage>();

    }
}