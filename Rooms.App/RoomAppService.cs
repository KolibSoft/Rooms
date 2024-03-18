using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core;

namespace KolibSoft.Rooms.App
{
    public class RoomAppService : RoomService
    {

        public RoomAppStatus Status { get; private set; } = RoomAppStatus.Offline;
        public RoomAppBehavior Behavior { get; set; } = RoomAppBehavior.Announce;
        public RoomAppManifest Manifest { get; set; } = new();
        public ImmutableArray<RoomAppConnection> Connections { get; private set; } = ImmutableArray.Create<RoomAppConnection>();
        public List<string> ConnectionCapabilities { get; } = new();

        public event EventHandler<RoomAppStatus>? StatusChanged;
        public event EventHandler<RoomAppManifest>? AppDiscovered;

        public async Task AnnounceAppAsync(RoomChannel? channel = default)
        {
            var json = JsonSerializer.Serialize(Manifest);
            await SendAsync(new RoomMessage
            {
                Verb = RoomAppVerbs.AppAnnouncement,
                Channel = channel ?? RoomChannel.Broadcast,
                Content = RoomContent.Parse(json)
            });
        }

        public async Task DiscoverApp(RoomChannel? channel = default)
        {
            var json = JsonSerializer.Serialize(ConnectionCapabilities);
            await SendAsync(new RoomMessage
            {
                Verb = RoomAppVerbs.AppDiscovering,
                Channel = channel ?? RoomChannel.Broadcast,
                Content = RoomContent.Parse(json)
            });
        }

        protected override async void OnConnect(IRoomSocket socket)
        {
            base.OnConnect(socket);
            StatusChanged?.Invoke(this, Status = RoomAppStatus.Online);
            if (Behavior == RoomAppBehavior.Announce) await AnnounceAppAsync();
        }

        protected override async void OnMessageReceived(RoomMessage message)
        {
            base.OnMessageReceived(message);
            if (message.Verb == RoomAppVerbs.AppAnnouncement)
            {
                var json = message.Content.ToString();
                var manifest = JsonSerializer.Deserialize<RoomAppManifest>(json);
                if (manifest != null)
                {
                    var capable = ConnectionCapabilities.Any(x => manifest.Capabilities.Contains(x));
                    var connection = Connections.FirstOrDefault(x => x.AppManifest.Id == manifest.Id);
                    if (connection == null && capable)
                    {
                        Connections = Connections.Add(new RoomAppConnection
                        {
                            AppManifest = manifest,
                            Channel = message.Channel
                        });
                        AppDiscovered?.Invoke(this, manifest);
                    }
                    else if (connection?.Channel == message.Channel && capable)
                    {
                        connection.AppManifest = manifest;
                        AppDiscovered?.Invoke(this, manifest);
                    }
                    else if (connection != null)
                    {
                        Connections = Connections.Remove(connection);
                    }
                }
            }
            else if (message.Verb == RoomAppVerbs.AppDiscovering && (Behavior == RoomAppBehavior.Announce || Behavior == RoomAppBehavior.AnnounceResponse))
            {
                var json = message.Content.ToString();
                var capabilities = JsonSerializer.Deserialize<string[]>(json);
                if (capabilities != null && Manifest.Capabilities.Any(x => capabilities.Contains(x)))
                    await AnnounceAppAsync(message.Channel);
            }
        }

        protected override void OnDisconnect(IRoomSocket socket)
        {
            base.OnDisconnect(socket);
            StatusChanged?.Invoke(this, Status = RoomAppStatus.Offline);
        }

    }
}
