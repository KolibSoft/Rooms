namespace KolibSoft.Rooms.Core.Protocol
{
    public sealed class RoomProtocol
    {
        public RoomVerb Verb { get; set; }
        public RoomChannel Channel { get; set; }
        public RoomCount Count { get; set; }
    }
}