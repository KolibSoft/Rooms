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
        protected IEnumerable<IRoomStream> Streams => _streams;
        protected bool IsDisposed => _disposed;

        protected virtual ValueTask OnReceiveAsync(IRoomStream stream, RoomMessage message, CancellationToken token) => ValueTask.CompletedTask;

        public virtual async ValueTask ListenAsync(IRoomStream stream, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running) throw new InvalidOperationException("Service is stopped");
            if (_streams.Contains(stream)) throw new InvalidOperationException("Stream already listening");
            _streams = _streams.Add(stream);
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
                        await Task.Delay(TimeSpan.FromSeconds(rate / Options.MaxStreamRate));
                    await OnReceiveAsync(stream, message, token);
                }
            }
            catch (Exception error)
            {
                if (Logger != null) await Logger.WriteLineAsync($"Error receiving message: {error}");
            }
            _streams = _streams.Remove(stream);
        }

        protected virtual async ValueTask OnSendAsync(IRoomStream stream, RoomMessage message, CancellationToken token)
        {
            await stream.WriteMessageAsync(message, token);
        }

        public virtual async ValueTask SendAsync(int index, RoomMessage message, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running) throw new InvalidOperationException("Service is stopped");
            if (_streams.Length == 0) return;
            if (index == -1)
            {
                foreach (var stream in _streams)
                {
                    message.Content.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        await OnSendAsync(stream, message, token);
                    }
                    catch (Exception error)
                    {
                        if (Logger != null) await Logger.WriteLineAsync($"Error sending message: {error}");
                    }
                }
            }
            else if (index < _streams.Length)
            {
                var stream = _streams[index];
                try
                {
                    await OnSendAsync(stream, message, token);
                }
                catch (Exception error)
                {
                    if (Logger != null) await Logger.WriteLineAsync($"Error sending message: {error}");
                }
            }
        }

        protected virtual void OnStart() { }
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

        private ImmutableArray<IRoomStream> _streams = ImmutableArray.Create<IRoomStream>();
        private bool _running = false;
        private bool _disposed = false;

    }
}