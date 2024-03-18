using KolibSoft.Rooms.Core;

namespace KolibSoft.Rooms.App
{
    public static class RoomAppVerbs
    {
        public static readonly RoomVerb AppAnnouncement = RoomVerb.Parse("AAV");
        public static readonly RoomVerb AppDiscovering = RoomVerb.Parse("ADV");
    }
}