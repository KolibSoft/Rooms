using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Streams;

namespace KolibSoft.Rooms.Core.Services
{
    public class RoomHub : RoomService
    {

        protected override void OnMessageReceived(IRoomStream stream, RoomProtocol protocol, Stream content)
        {
            _messages = _messages.Enqueue(new RoomMessage(stream, protocol, content));
        }

        protected override void OnMessageSent(IRoomStream stream, RoomProtocol protocol, Stream content) { }

        protected virtual async void OnProcessLoopbackMessage(RoomMessage message)
        {
            await message.Source.WriteProtocolAsync(message.Protocol);
            await message.Source.WriteContentAsync((long)message.Protocol.Count, message.Content);
        }

        private async void TransmitAsync()
        {
            while (IsRunning)
            {
                if (_messages.Any())
                {
                    _messages = _messages.Dequeue(out RoomMessage message);
                    var channel = (int)message.Protocol.Channel;
                    if (channel == 0)
                        try
                        {
                            OnProcessLoopbackMessage(message);
                        }
                        catch (Exception error)
                        {
                            if (Logger != null) await Logger.WriteLineAsync($"Error trasnmiting message: {error}");
                        }
                    else if (channel == -1)
                    {
                        var hash = message.Source.GetHashCode();
                        var count = (long)message.Protocol.Count;
                        var clone = message.Content;
                        if (!clone.CanSeek)
                        {
                            if (count < Options.MaxFastBuffering)
                            {
                                clone = new MemoryStream((int)count);
                                await message.Content.CopyToAsync(clone);
                            }
                            else
                            {
                                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), $"{DateTime.UtcNow.Ticks}");
                                clone = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
                                await message.Content.CopyToAsync(clone);
                            }
                        }
                        foreach (var stream in Streams)
                            if (stream != message.Source)
                            {
                                clone.Seek(0, SeekOrigin.Begin);
                                message.Protocol.Channel = (RoomChannel)(hash ^ stream.GetHashCode());
                                try
                                {
                                    await stream.WriteProtocolAsync(message.Protocol);
                                    await stream.WriteContentAsync((long)message.Protocol.Count, clone);
                                }
                                catch (Exception error)
                                {
                                    if (Logger != null) await Logger.WriteLineAsync($"Error trasnmiting message: {error}");
                                }
                            }
                    }
                    else
                    {
                        var hash = message.Source.GetHashCode() ^ channel;
                        var target = Streams.FirstOrDefault(x => x.GetHashCode() == hash);
                        if (target != null)
                        {
                            try
                            {
                                await target.WriteProtocolAsync(message.Protocol);
                                await target.WriteContentAsync((long)message.Protocol.Count, message.Content);
                            }
                            catch (Exception error)
                            {
                                if (Logger != null) await Logger.WriteLineAsync($"Error trasnmiting message: {error}");
                            }
                        }
                    }
                }
                else await Task.Delay(100);
            }
        }

        protected override void OnStart() => TransmitAsync();

        public RoomHub(RoomServiceOptions? options = null) : base(options) { }

        private ImmutableQueue<RoomMessage> _messages = ImmutableQueue.Create<RoomMessage>();

    }
}