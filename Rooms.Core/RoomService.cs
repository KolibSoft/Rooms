using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core
{

    public delegate Task<IRoomSocket> SocketConnector(string server);

    public class RoomService : IDisposable
    {

        private bool disposed;
        public IRoomSocket? Socket { get; private set; }
        public Dictionary<string, SocketConnector> Connectors { get; } = new();

        protected virtual void OnConnect(IRoomSocket socket) { }
        public async Task ConnectAsync(string server, string implementation)
        {
            if (disposed) throw new ObjectDisposedException(null);
            try
            {
                await DisconnectAsync();
                if (Connectors.TryGetValue(implementation, out SocketConnector? connector))
                {
                    var socket = await connector.Invoke(server);
                    OnConnect(socket);
                    ListenAsync(socket);
                    Socket = socket;
                }
            }
            catch { }
        }

        protected virtual void OnMessageSent(RoomMessage message) { }
        public async Task SendAsync(RoomMessage message)
        {
            if (disposed) throw new ObjectDisposedException(null);
            var socket = Socket;
            if (socket?.IsAlive == true)
                try
                {
                    await socket.SendAsync(message);
                    OnMessageSent(message);
                }
                catch { }
        }

        protected virtual void OnMessageReceived(RoomMessage message) { }
        private async void ListenAsync(IRoomSocket socket)
        {
            while (socket.IsAlive)
            {
                try
                {
                    var message = await socket.ReceiveAsync();
                    OnMessageReceived(message);
                }
                catch { }
                await Task.Delay(100);
            }
            OnDisconnect(socket);
            socket.Dispose();
        }

        protected virtual void OnDisconnect(IRoomSocket socket) { }
        public Task DisconnectAsync()
        {
            if (disposed) throw new ObjectDisposedException(null);
            var socket = Socket;
            if (socket?.IsAlive == true)
            {
                OnDisconnect(socket);
                socket.Dispose();
            }
            return Task.CompletedTask;
        }

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

        public RoomService()
        {
            Connectors[TCP] = async (server) =>
            {
                var cancellation = new CancellationTokenSource();
                cancellation.CancelAfter(TimeSpan.FromMinutes(1));
                var parts = server.Split(":");
                var host = (await Dns.GetHostAddressesAsync(parts[0])).First();
                var port = int.Parse(parts[1]);
                var client = new TcpClient();
                await client.ConnectAsync(host, port, cancellation.Token);
                var socket = new TcpRoomSocket(client);
                return socket;
            };
            Connectors[WEB] = async (server) =>
            {
                var cancellation = new CancellationTokenSource();
                cancellation.CancelAfter(TimeSpan.FromMinutes(1));
                var uri = new Uri(server);
                var client = new ClientWebSocket();
                client.Options.AddSubProtocol(WebRoomSocket.SubProtocol);
                await client.ConnectAsync(uri, cancellation.Token);
                var socket = new WebRoomSocket(client);
                return socket;
            };
        }

        public const string TCP = "TCP";
        public const string WEB = "WEB";

    }
}