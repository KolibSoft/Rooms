using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
        public bool IsRunning => _running;
        protected IEnumerable<MessageContext> Messages => _messages;
        protected bool IsDisposed => _disposed;

        protected abstract ValueTask OnReceiveAsync(IRoomStream stream, RoomMessage message, CancellationToken token);

        public virtual async ValueTask ListenAsync(IRoomStream stream, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running) throw new InvalidOperationException("Service not running");
            try
            {
                var ttl = TimeSpan.FromSeconds(1);
                var stopwatch = new Stopwatch();
                var rate = 0L;
                stopwatch.Start();
                while (_running && stream.IsAlive)
                {
                    var message = await stream.ReadMessageAsync(token);
                    if (stopwatch.Elapsed >= ttl)
                    {
                        rate = 0;
                        stopwatch.Restart();
                    }
                    rate += message.Content.Length;
                    if (rate > Options.MaxStreamRate)
                        await Task.Delay(TimeSpan.FromSeconds(rate / Options.MaxStreamRate), token);
                    await OnReceiveAsync(stream, message, token);
                    if (!Messages.Any(x => x.Message.Content == message.Content))
                        await message.Content.DisposeAsync();
                }
            }
            catch (Exception error)
            {
                if (Logger != null) await Logger.WriteLineAsync($"Error receiving message: {error}");
            }
        }

        protected virtual async ValueTask OnSendAsync(IRoomStream stream, RoomMessage message, CancellationToken token)
        {
            await stream.WriteMessageAsync(message, token);
        }

        public virtual void Enqueue(IRoomStream stream, RoomMessage message)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running) throw new InvalidOperationException("Service not running");
            _messages = _messages.Enqueue(new MessageContext(stream, message));
        }

        private async void Transmit()
        {
            while (_running)
                if (_messages.Any())
                {
                    _messages = _messages.Dequeue(out MessageContext context);
                    try
                    {
                        await OnSendAsync(context.Stream, context.Message, default);
                        if (!Messages.Any(x => x.Message.Content == context.Message.Content))
                            await context.Message.Content.DisposeAsync();
                    }
                    catch (Exception error)
                    {
                        if (Logger != null) await Logger.WriteLineAsync($"Error sending message: {error}");
                    }
                }
                else await Task.Delay(100);
        }

        protected virtual void OnStart() => Transmit();
        public void Start()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running)
            {
                _running = true;
                OnStart();
            }
        }

        protected virtual void OnStop() { }
        public void Stop()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (_running)
            {
                _running = false;
                OnStop();
            }
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

        private ImmutableQueue<MessageContext> _messages = ImmutableQueue.Create<MessageContext>();
        private bool _running = false;
        private bool _disposed = false;

        protected readonly struct MessageContext
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