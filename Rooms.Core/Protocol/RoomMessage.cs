using System;

namespace KolibSoft.Rooms.Core.Protocol
{

    public class RoomMessage
    {

        public RoomVerb Verb { get; set; }
        public RoomChannel Channel { get; set; }
        public RoomContent Content { get; set; }

        public int Length => Verb.Length + Channel.Length + Content.Length + 2;

        public override string ToString() => $"{Verb} {Channel}\n{Content}";

        public void CopyTo(Span<byte> target)
        {
            var offset = 0;
            Verb.CopyTo(target[offset..]);
            offset += Verb.Length;
            target[offset] = (byte)' ';
            offset += 1;
            Channel.CopyTo(target[offset..]);
            offset += Channel.Length;
            target[offset] = (byte)'\n';
            offset += 1;
            Content.CopyTo(target[offset..]);
        }

        public void CopyFrom(ReadOnlySpan<byte> source)
        {
            var offset = 0;
            var length = RoomVerb.Scan(source);
            Verb = new RoomVerb(source[offset..(offset + length)].ToArray());
            offset += Verb.Length + 1;
            length = RoomChannel.Scan(source);
            Channel = new RoomChannel(source[offset..(offset + length)].ToArray());
            offset += Channel.Length + 1;
            Content = new RoomContent(source[offset..].ToArray());
        }

    }

}