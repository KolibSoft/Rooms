using System;
using System.Collections.Immutable;
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
            _messages = _messages.Enqueue(new MessageContext(stream, message));
            return ValueTask.CompletedTask;
        }

        private async void TransmitAsync()
        {
            while (IsRunning)
            {
                if (_messages.Any())
                {
                    _messages = _messages.Dequeue(out MessageContext context);
                    var channel = context.Message.Channel;
                    if (channel == 0)
                        try
                        {
                            await OnSendAsync(context.Stream, context.Message, default);
                        }
                        catch (Exception error)
                        {
                            if (Logger != null) await Logger.WriteLineAsync($"Error trasnmiting message: {error}");
                        }
                    else if (channel == -1)
                    {
                        var hash = context.Stream.GetHashCode();
                        foreach (var stream in Streams)
                            if (stream != context.Stream)
                            {
                                context.Message.Content.Seek(0, SeekOrigin.Begin);
                                context.Message.Channel = hash ^ stream.GetHashCode();
                                try
                                {
                                    await OnSendAsync(stream, context.Message, default);
                                }
                                catch (Exception error)
                                {
                                    if (Logger != null) await Logger.WriteLineAsync($"Error trasnmiting message: {error}");
                                }
                            }
                    }
                    else
                    {
                        var hash = context.Stream.GetHashCode() ^ channel;
                        var target = Streams.FirstOrDefault(x => x.GetHashCode() == hash);
                        if (target != null)
                            try
                            {
                                await OnSendAsync(target, context.Message, default);
                            }
                            catch (Exception error)
                            {
                                if (Logger != null) await Logger.WriteLineAsync($"Error trasnmiting message: {error}");
                            }
                    }
                }
                else await Task.Delay(100);
            }
        }

        protected override void OnStart() => TransmitAsync();

        public RoomHub(RoomServiceOptions? options = null) : base(options) { }

        private ImmutableQueue<MessageContext> _messages = ImmutableQueue.Create<MessageContext>();

        private readonly struct MessageContext
        {

            public readonly IRoomStream Stream;
            public readonly RoomMessage Message;

            public MessageContext(IRoomStream stream, RoomMessage message)
            {
                Stream = stream;
                Message = message;
            }

        }

    }
}