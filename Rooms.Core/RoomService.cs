using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// Manage a Room connection.
    /// </summary>
    public class RoomService : IDisposable
    {

        /// <summary>
        /// Async monitor
        /// </summary>
        private object monitor = new();

        /// <summary>
        /// Disposed flag.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The underlying socket implementation.
        /// </summary>
        public IRoomSocket? Socket { get; private set; }

        /// <summary>
        /// Service status.
        /// </summary>
        public RoomServiceStatus Status { get; private set; }

        /// <summary>
        /// Available socket implementations.
        /// </summary>
        public Dictionary<string, RoomConnector> Connectors { get; } = new();

        /// <summary>
        /// Service status changed event.
        /// </summary>
        public event EventHandler<RoomServiceStatus>? StatusChanged;

        /// <summary>
        /// Log writer
        /// </summary>
        public TextWriter? LogWriter { get; set; }

        /// <summary>
        /// Called just after success socket connection.
        /// </summary>
        /// <param name="socket">Connected socket.</param>
        protected virtual void OnConnect(IRoomSocket socket) { }

        /// <summary>
        /// Attempts to establish a connection with the relay server.
        /// </summary>
        /// <param name="server">Implementation server identifier.</param>
        /// <param name="implementation">Implementation connector name.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task ConnectAsync(string server, string implementation)
        {
            if (disposed) throw new ObjectDisposedException(null);
            try
            {
                await DisconnectAsync();
                if (Connectors.TryGetValue(implementation, out RoomConnector? connector))
                {
                    var socket = await connector.Invoke(server);
                    lock (monitor)
                    {
                        Socket = socket;
                        Status = RoomServiceStatus.Online;
                        StatusChanged?.Invoke(this, RoomServiceStatus.Online);
                        OnConnect(socket);
                    }
                    ListenAsync(socket);
                }
            }
            catch (Exception e)
            {
                var writer = LogWriter;
                if (writer != null) await writer.WriteAsync($"Room Service exception: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Called on message sent.
        /// </summary>
        /// <param name="message">Message sent.</param>
        protected virtual void OnMessageSent(RoomMessage message) { }

        /// <summary>
        /// Attempts to send a message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
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
                catch (Exception e)
                {
                    var writer = LogWriter;
                    if (writer != null) await writer.WriteAsync($"Room Service exception: {e.Message}\n{e.StackTrace}");
                }
        }

        /// <summary>
        /// Called on message received.
        /// </summary>
        /// <param name="message">Message received</param>
        protected virtual void OnMessageReceived(RoomMessage message) { }

        /// <summary>
        /// Start to listen incoming messages.
        /// </summary>
        /// <param name="socket">Connected socket.</param>
        /// <param name="rateLimit">Max amount of bytes to read per second.</param>
        private async void ListenAsync(IRoomSocket socket, int rateLimit = 1024)
        {
            var bytes = 0;
            var ttl = TimeSpan.FromSeconds(1);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (socket.IsAlive)
            {
                try
                {
                    var message = await socket.ReceiveAsync();
                    bytes += message.Length;
                    if (bytes > rateLimit)
                    {
                        socket.Dispose();
                        break;
                    }
                    if (stopwatch.Elapsed > ttl)
                    {
                        bytes = 0;
                        stopwatch.Restart();
                    }
                    OnMessageReceived(message);
                }
                catch (Exception e)
                {
                    var writer = LogWriter;
                    if (writer != null) await writer.WriteAsync($"Room Service exception: {e.Message}\n{e.StackTrace}");
                }
            }
            stopwatch.Stop();
            lock (monitor)
            {
                OnDisconnect(socket);
                socket.Dispose();
                if (Socket == socket)
                {
                    Socket = null;
                    Status = RoomServiceStatus.Offline;
                    StatusChanged?.Invoke(this, RoomServiceStatus.Offline);
                }
            }
        }

        /// <summary>
        /// Called just before socket disconnection.
        /// </summary>
        /// <param name="socket">Disconnected socket.</param>
        protected virtual void OnDisconnect(IRoomSocket socket) { }

        /// <summary>
        /// Ends the connection with the relay server.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public Task DisconnectAsync()
        {
            if (disposed) throw new ObjectDisposedException(null);
            lock (monitor)
            {
                if (Socket?.IsAlive == true)
                {
                    OnDisconnect(Socket);
                    Socket.Dispose();
                    Socket = null;
                    Status = RoomServiceStatus.Offline;
                    StatusChanged?.Invoke(this, RoomServiceStatus.Offline);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Internal dispose implementation.
        /// </summary>
        /// <param name="disposing"></param>
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

        /// <summary>
        /// Constructs a service with the generic TCP and WEB connector implementations.
        /// </summary>
        public RoomService()
        {
            Connectors[TCP] = TcpConnector;
            Connectors[WEB] = WebConnector;
        }

        /// <summary>
        /// TCP implementation name.
        /// </summary>
        public const string TCP = "TCP";

        /// <summary>
        /// TCP implementation connector.
        /// </summary>
        public static readonly RoomConnector TcpConnector = async (server) =>
            {
                server = server.Replace("localhost", "127.0.0.1");
                var cancellation = new CancellationTokenSource();
                cancellation.CancelAfter(TimeSpan.FromMinutes(1));
                var endpoint = IPEndPoint.Parse(server);
                var client = new TcpClient();
                await client.ConnectAsync(endpoint.Address, endpoint.Port, cancellation.Token);
                var socket = new TcpRoomSocket(client);
                return socket;
            };

        /// <summary>
        /// WEB implementation name.
        /// </summary>
        public const string WEB = "WEB";

        /// <summary>
        /// WEB implementation connector.
        /// </summary>
        public static readonly RoomConnector WebConnector = async (server) =>
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
}