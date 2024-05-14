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
    public abstract class RoomService : IRoomService
    {

        public RoomServiceOptions Options { get; private set; }
        public TextWriter? Logger { get; set; }
        protected bool IsDisposed => _disposed;

        protected abstract void OnMessageReceived(IRoomStream stream, RoomProtocol protocol, Stream content);
        public virtual async ValueTask ListenAsync(IRoomStream stream, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running) throw new InvalidOperationException("Service is stopped");
            if (!_streams.Contains(stream))
            {
                _streams = _streams.Add(stream);
                try
                {
                    while (_running && stream.IsAlive)
                    {
                        var protocol = new RoomProtocol();
                        await stream.ReadProtocolAsync(protocol, token);
                        var count = (long)protocol.Count;
                        var content = Stream.Null;
                        if (count < Options.MaxFastBuffering)
                        {
                            content = new MemoryStream((int)count);
                            await stream.ReadContentAsync(count, content, token);
                        }
                        else
                        {
                            content = new FileStream($"{DateTime.UtcNow.Ticks}", FileMode.Create, FileAccess.ReadWrite);
                            await stream.ReadContentAsync(count, content, token);
                        }
                        content.Seek(0, SeekOrigin.Begin);
                        OnMessageReceived(stream, protocol, content);
                    }
                }
                catch (Exception error)
                {
                    if (Logger != null) await Logger.WriteLineAsync($"Error receiving message: {error}");
                }
                _streams = _streams.Remove(stream);
            }
        }

        protected abstract void OnMessageSent(IRoomStream stream, RoomProtocol protocol, Stream content);
        public virtual async ValueTask SendAsync(RoomProtocol protocol, Stream content, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running) throw new InvalidOperationException("Service is stopped");
            if (_streams.Length == 1)
            {
                var stream = _streams.First();
                try
                {
                    await stream.WriteProtocolAsync(protocol, token);
                    await stream.WriteContentAsync((long)protocol.Count, content, token);
                    OnMessageSent(stream, protocol, content);
                }
                catch (Exception error)
                {
                    if (Logger != null) await Logger.WriteLineAsync($"Error sending message: {error}");
                }
            }
            else
            {
                var count = (long)protocol.Count;
                var clone = Stream.Null;
                if (count < Options.MaxFastBuffering)
                {
                    clone = new MemoryStream((int)count);
                    await content.CopyToAsync(clone, token);
                }
                else
                {
                    clone = new FileStream($"{DateTime.UtcNow.Ticks}", FileMode.Create, FileAccess.ReadWrite);
                    await content.CopyToAsync(clone, token);
                }
                foreach (var stream in _streams)
                {
                    clone.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        await stream.WriteProtocolAsync(protocol, token);
                        await stream.WriteContentAsync(count, content, token);
                        OnMessageSent(stream, protocol, content);
                    }
                    catch (Exception error)
                    {
                        if (Logger != null) await Logger.WriteLineAsync($"Error sending message: {error}");
                    }
                }
            }
        }

        public ValueTask StartAsync()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running)
            {
                _running = true;
            }
            return ValueTask.CompletedTask;
        }

        public ValueTask StopAsync()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (_running)
            {
                _running = false;
            }
            return ValueTask.CompletedTask;
        }

        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                _running = false;
                _disposed = true;
            }
            return ValueTask.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async void Dispose()
        {
            await DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected RoomService(RoomServiceOptions? options = null)
        {
            Options = options ?? new RoomServiceOptions();
        }

        private ImmutableArray<IRoomStream> _streams = ImmutableArray.Create<IRoomStream>();
        private bool _running = false;
        private bool _disposed = false;

    }
}