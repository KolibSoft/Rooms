using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Sockets;

namespace KolibSoft.Rooms.Core.Services
{

    public class RoomHub : IDisposable
    {

        private bool disposed;

        public ImmutableArray<IRoomSocket> Sockets { get; private set; } = ImmutableArray.Create<IRoomSocket>();
        public ImmutableQueue<RoomContext> Messages { get; private set; } = ImmutableQueue.Create<RoomContext>();

        public TextWriter? Logger { get; set; }

        public async Task ListenAsync(IRoomSocket socket, int rating = 1024)
        {
            if (disposed) throw new ObjectDisposedException(null);
            Sockets = Sockets.Add(socket);
            var message = new RoomMessage();
            var ttl = TimeSpan.FromSeconds(1);
            var stopwatch = new Stopwatch();
            var rate = 0;
            stopwatch.Start();
            while (socket.IsAlive)
            {
                try
                {
                    await socket.ReceiveAsync(message);
                    rate += message.Length;
                    if (rate > rating)
                    {
                        socket.Dispose();
                        break;
                    }
                    if (stopwatch.Elapsed >= ttl)
                    {
                        rate = 0;
                        stopwatch.Restart();
                    }
                    Messages = Messages.Enqueue(new RoomContext(socket, message));
                }
                catch (Exception error)
                {
                    if (Logger != null) await Logger.WriteLineAsync($"Room Hub error: {error}");
                }
            }
            Sockets = Sockets.Remove(socket);
        }

        public async Task TransmitAsync()
        {
            if (disposed) throw new ObjectDisposedException(null);
            while (Sockets.Any())
            {
                while (Messages.Any())
                {
                    Messages = Messages.Dequeue(out RoomContext context);
                    if (context.Message.Channel == RoomChannel.Loopback)
                    {
                        try
                        {
                            await context.Socket.SendAsync(context.Message);
                        }
                        catch (Exception error)
                        {
                            if (Logger != null) await Logger.WriteLineAsync($"Room Hub error: {error}");
                        }
                    }
                    else if (context.Message.Channel == RoomChannel.Broadcast)
                    {
                        var hash = context.Socket.GetHashCode();
                        foreach (var socket in Sockets)
                            if (socket != context.Socket)
                                try
                                {
                                    context.Message.Channel = hash ^ socket.GetHashCode();
                                    await socket.SendAsync(context.Message);
                                }
                                catch (Exception error)
                                {
                                    if (Logger != null) await Logger.WriteLineAsync($"Room Hub error: {error}");
                                }
                    }
                    else
                    {
                        var hash = context.Message.Channel ^ context.Socket.GetHashCode();
                        var socket = Sockets.FirstOrDefault(x => x.GetHashCode() == hash);
                        if (socket != null)
                            try
                            {
                                await socket.SendAsync(context.Message);
                            }
                            catch (Exception error)
                            {
                                if (Logger != null) await Logger.WriteLineAsync($"Room Hub error: {error}");
                            }
                    }
                }
                await Task.Delay(100);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing) foreach (var socket in Sockets) socket.Dispose();
                Sockets = Sockets.Clear();
                Messages = Messages.Clear();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

}