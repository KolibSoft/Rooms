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

    /// <summary>
    /// Base class to implement Room services.
    /// </summary>
    public class RoomService : IDisposable
    {

        /// <summary>
        /// Dipose flag.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Available connector implementations.
        /// </summary>
        public Dictionary<string, RoomConnector> Connectors { get; }

        /// <summary>
        /// Current working socket.
        /// </summary>
        public IRoomSocket? Socket { get; private set; }

        /// <summary>
        /// Checks if the underlying socket is alive.
        /// </summary>
        public bool IsOnline => Socket?.IsAlive == true;

        /// <summary>
        /// Writer to report errors.
        /// </summary>
        public TextWriter? Logger { get; set; }

        /// <summary>
        /// Connect to a Room server using the specific implementation.
        /// </summary>
        /// <param name="connstring">Implementation specific connection string.</param>
        /// <param name="impl">Implementation identifier.</param>
        /// <param name="rating">Max amount of bytes received per second.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">If the service was disposed.</exception>
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
                    _ = ListenAsync(Socket, rating);
                }
            }
            catch (Exception error)
            {
                if (Logger != null) await Logger.WriteLineAsync($"Room Service error: {error}");
            }
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">If the service was disposed.</exception>
        public async Task DisconnectAsync()
        {
            if (disposed) throw new ObjectDisposedException(null);
            try
            {
                Socket?.Dispose();
            }
            catch (Exception error)
            {
                if (Logger != null) await Logger.WriteLineAsync($"Room Service error: {error}");
            }
        }

        /// <summary>
        /// Called after open a connection.
        /// </summary>
        /// <param name="socket"></param>
        protected virtual void OnOnline(IRoomSocket socket) { }

        /// <summary>
        /// Called after receive a message.
        /// </summary>
        /// <param name="message"></param>
        protected virtual void OnMessageReceived(RoomMessage message) { }

        /// <summary>
        /// Called after close a connection.
        /// </summary>
        /// <param name="socket"></param>
        protected virtual void OnOffline(IRoomSocket socket) { }

        /// <summary>
        /// Start listen the socket incoming messages.
        /// </summary>
        /// <param name="socket">Socket to listen.</param>
        /// <param name="rating">Max amount of bytes received per second.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">If the service was disposed.</exception>
        private async Task ListenAsync(IRoomSocket socket, int rating = 1024)
        {
            OnOnline(socket);
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
            OnOffline(socket);
        }

        /// <summary>
        /// Called after send a message.
        /// </summary>
        /// <param name="message"></param>
        protected virtual void OnMessageSent(RoomMessage message) { }

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">If the service was disposed.</exception>
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
        /// Constructs a default service.
        /// </summary>
        public RoomService()
        {
            Connectors = new Dictionary<string, RoomConnector>
            {
                [TCP] = TcpConnector,
                [WEB] = WebConnector
            };
        }

        /// <summary>
        /// TCP implementation identifier.
        /// </summary>
        public const string TCP = "TCP";

        /// <summary>
        /// WEB implementatuon identifier.
        /// </summary>
        public const string WEB = "WEB";

        /// <summary>
        /// Default TCP implementation connector.
        /// </summary>
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

        /// <summary>
        /// Default WEB implementation connector.
        /// </summary>
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

    }

}