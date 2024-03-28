using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Sockets;

namespace KolibSoft.Rooms.Core.Services
{

    public class RoomHub
    {

        public ImmutableArray<IRoomSocket> Sockets { get; private set; } = ImmutableArray.Create<IRoomSocket>();
        public ImmutableQueue<RoomMessage> Messages { get; private set; } = ImmutableQueue.Create<RoomMessage>();

        public TextWriter? Logger { get; set; }

        public async Task ListenAsync(IRoomSocket socket)
        {
            Sockets = Sockets.Add(socket);
            var message = new RoomMessage();
            while (socket.IsAlive)
            {
                try
                {
                    await socket.ReceiveAsync(message);
                    Messages = Messages.Enqueue(message);
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
            while (Sockets.Any())
            {
                while (Messages.Any())
                {
                    Messages = Messages.Dequeue(out RoomMessage message);
                    foreach (var socket in Sockets)
                    {
                        try
                        {
                            await socket.SendAsync(message);
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

    }

}