using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public class RoomMessage
    {

        public RoomVerb Verb { get; set; }
        public RoomChannel Channel { get; set; }
        public RoomContent Content { get; set; }

        public int Length => Verb.Length + Channel.Length + Content.Length + 2;

        public override string ToString() => $"{Verb} {Channel} {Content}";

        public void CopyTo(Span<byte> target)
        {
            var offset = 0;
            Verb.CopyTo(target[offset..]);
            offset += Verb.Length;
            target[offset] = (byte)' ';
            offset += 1;
            Channel.CopyTo(target[offset..]);
            offset += Channel.Length;
            if (Content.Length > 0)
            {
                target[offset] = (byte)' ';
                Content.CopyTo(target[(offset + 1)..]);
            }
        }

        public void CopyFrom(ReadOnlySpan<byte> source)
        {
            var offset = 0;
            var length = RoomVerb.Scan(source[offset..]);
            Verb = new RoomVerb(source[offset..(offset + length)].ToArray());
            offset += Verb.Length + 1;
            length = RoomChannel.Scan(source[offset..]);
            Channel = new RoomChannel(source[offset..(offset + length)].ToArray());
            offset += Channel.Length;
            if (offset < source.Length) Content = new RoomContent(source[(offset + 1)..].ToArray());
        }

        public RoomMessage(ArraySegment<byte> data)
        {
            var offset = 0;
            var length = RoomVerb.Scan(data[offset..]);
            Verb = new RoomVerb(data[offset..(offset + length)]);
            offset += Verb.Length + 1;
            length = RoomChannel.Scan(data[offset..]);
            Channel = new RoomChannel(data[offset..(offset + length)]);
            offset += Channel.Length;
            if (offset < data.Count) Content = new RoomContent(data[(offset + 1)..]);
        }

        public RoomMessage() {}

        public static bool Verify(ReadOnlySpan<byte> utf8)
        {
            var offset = 0;
            var index = RoomVerb.Scan(utf8);
            if (index == 0 || index == utf8.Length) return false;
            offset += index;
            if (!char.IsWhiteSpace((char)utf8[offset])) return false;
            offset++;
            index = RoomChannel.Scan(utf8[offset..]);
            if (index == 0 || index == utf8.Length) return false;
            offset += index;
            if (offset < utf8.Length && !char.IsWhiteSpace((char)utf8[offset])) return false;
            return true;
        }

        public static bool Verify(ReadOnlySpan<char> chars)
        {
            var offset = 0;
            var index = RoomVerb.Scan(chars);
            if (index == 0 || index == chars.Length) return false;
            offset += index;
            if (!char.IsWhiteSpace(chars[offset])) return false;
            offset++;
            index = RoomChannel.Scan(chars[offset..]);
            if (index == 0 || index == chars.Length) return false;
            offset += index;
            if (offset < chars.Length && !char.IsWhiteSpace(chars[offset])) return false;
            return true;
        }

        public static bool TryParse(ReadOnlySpan<byte> utf8, [NotNullWhen(true)] out RoomMessage? message)
        {
            if (Verify(utf8))
            {
                var data = new byte[utf8.Length];
                utf8.CopyTo(data);
                message = new RoomMessage(data);
                return true;
            }
            else
            {
                message = null;
                return false;
            }
        }

        public static bool TryParse(ReadOnlySpan<char> chars, [NotNullWhen(true)] out RoomMessage? message)
        {
            if (Verify(chars))
            {
                var data = new byte[Encoding.UTF8.GetByteCount(chars)];
                Encoding.UTF8.GetBytes(chars, data);
                message = new RoomMessage(data);
                return true;
            }
            else
            {
                message = null;
                return false;
            }
        }

        public static RoomMessage Parse(ReadOnlySpan<byte> utf8)
        {
            if (TryParse(utf8, out RoomMessage? message)) return message;
            throw new FormatException($"Invalid message format: {Encoding.UTF8.GetString(utf8)}");
        }

        public static RoomMessage Parse(ReadOnlySpan<char> chars)
        {
            if (TryParse(chars, out RoomMessage? message)) return message;
            throw new FormatException($"Invalid message format: {new string(chars)}");
        }

    }

}