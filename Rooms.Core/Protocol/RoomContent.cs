using System;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomContent
    {

        internal readonly ArraySegment<byte> data;

        public override string ToString() => Encoding.UTF8.GetString(data);

        public override int GetHashCode() => data.GetHashCode();

        public override bool Equals(object? obj)
        {
            var result = (obj is RoomContent content) && this == content;
            return result;
        }

        public RoomContent(ArraySegment<byte> data)
        {
            this.data = data;
        }

        public static bool operator ==(RoomContent lhs, RoomContent rhs)
        {
            var result = lhs.data.SequenceEqual(rhs.data);
            return result;
        }

        public static bool operator !=(RoomContent lhs, RoomContent rhs)
        {
            var result = !lhs.data.SequenceEqual(rhs.data);
            return result;
        }

        public static RoomContent Parse(ReadOnlySpan<byte> utf8)
        {
            var data = new byte[utf8.Length];
            utf8.CopyTo(data);
            var content = new RoomContent(data);
            return content;
        }

        public static RoomContent Parse(ReadOnlySpan<char> chars)
        {
            var data = new byte[Encoding.UTF8.GetByteCount(chars)];
            Encoding.UTF8.GetBytes(chars, data);
            var content = new RoomContent(data);
            return content;
        }

    }

}