namespace KolibSoft.Rooms.Core.Protocol
{

    public class RoomMessage
    {

        public RoomVerb Verb { get; set; }
        public RoomChannel Channel { get; set; }
        public RoomContent Content { get; set; }

        public override string ToString() => $"{Verb} {Channel} {Content.data.Count}\n{Content}";

    }

}