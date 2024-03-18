using KolibSoft.Rooms.Core;

namespace KolibSoft.Rooms.App
{
    public class RoomAppConnection
    {
        public RoomAppManifest AppManifest { get; set; } = new();
        public RoomChannel Channel { get; set; } = RoomChannel.Loopback;
    }
}