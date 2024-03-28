using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Sockets;

namespace KolibSoft.Rooms.Core.Services
{

    public class RoomService : IDisposable
    {

        private bool disposed;

        public Dictionary<string, RoomConnector> Connectors { get; }

        public IRoomSocket? Socket { get; private set; }

        public bool IsOnline => Socket?.IsAlive == true;

        public TextWriter? Logger { get; set; }

        protected virtual void OnConnect(IRoomSocket socket) { }
        public async Task ConnectAsync(string connstring, string impl, int rating = 1024)
        {
            if (disposed) throw new ObjectDisposedException(null);
            try
            {
                await DisconnectAsync();
                var connector = Connectors[impl];
                if (connector != null)
                {
                    Socket = await connector.Invoke(connstring);
                    OnConnect(Socket);
                    _ = ListenAsync(Socket, rating);
                }
            }
            catch (Exception error)
            {
                if (Logger != null) await Logger.WriteLineAsync($"Room Service error: {error}");
            }
        }

        protected virtual void OnDisconnect(IRoomSocket socket) { }
        public async Task DisconnectAsync()
        {
            if (disposed) throw new ObjectDisposedException(null);
            try
            {
                if (Socket != null)
                {
                    Socket.Dispose();
                    OnDisconnect(Socket);
                }
            }
            catch (Exception error)
            {
                if (Logger != null) await Logger.WriteLineAsync($"Room Service error: {error}");
            }
        }

        protected virtual void OnMessageReceived(RoomMessage message) { }
        private async Task ListenAsync(IRoomSocket socket, int rating = 1024)
        {
            if (disposed) throw new ObjectDisposedException(null);
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
                    OnMessageReceived(message);
                }
                catch (Exception error)
                {
                    if (Logger != null) await Logger.WriteLineAsync($"Room Service error: {error}");
                }
            }
            OnDisconnect(socket);
        }

        protected virtual void OnMessageSent(RoomMessage message) { }
        public async Task SendAsync(RoomMessage message)
        {
            if (disposed) throw new ObjectDisposedException(null);
            if (Socket != null)
                try
                {
                    await Socket.SendAsync(message);
                    OnMessageSent(message);
                }
                catch (Exception error)
                {
                    if (Logger != null) await Logger.WriteLineAsync($"Room Service error: {error}");
                }
        }

        public RoomService()
        {
            Connectors = new Dictionary<string, RoomConnector>
            {
                [TCP] = TcpConnector,
                [WEB] = WebConnector
            };
        }

        public const string TCP = "TCP";
        public const string WEB = "WEB";

        public static readonly RoomConnector TcpConnector = async (connstring) =>
        {
            connstring = connstring.Replace("localhost", "127.0.0.1");
            var cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(TimeSpan.FromMinutes(1.0));
            var endpoint = IPEndPoint.Parse(connstring);
            var client = new TcpClient();
            await client.ConnectAsync(endpoint.Address, endpoint.Port, cancellation.Token);
            return new TcpRoomSocket(client);
        };

        public static readonly RoomConnector WebConnector = async (connstring) =>
        {
            var cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(TimeSpan.FromMinutes(1.0));
            var uri = new Uri(connstring);
            var client = new ClientWebSocket();
            client.Options.AddSubProtocol(WebRoomSocket.SubProtocol);
            await client.ConnectAsync(uri, cancellation.Token);
            return new WebRoomSocket(client);
        };

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing) Socket?.Dispose();
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