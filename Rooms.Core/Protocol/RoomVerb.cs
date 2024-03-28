using System;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomVerb
    {

        internal readonly ArraySegment<byte> data;

        public int Length => data.Count;

        public override string ToString() => Encoding.UTF8.GetString(data);

        public override int GetHashCode() => data.GetHashCode();

        public override bool Equals(object? obj)
        {
            var result = (obj is RoomVerb verb) && this == verb;
            return result;
        }

        public RoomVerb(ArraySegment<byte> data)
        {
            this.data = data;
        }

        public static bool operator ==(RoomVerb lhs, RoomVerb rhs)
        {
            var result = lhs.data.SequenceEqual(rhs.data);
            return result;
        }

        public static bool operator !=(RoomVerb lhs, RoomVerb rhs)
        {
            var result = !lhs.data.SequenceEqual(rhs.data);
            return result;
        }

        public static int Scan(ReadOnlySpan<byte> utf8)
        {
            var index = 0;
            while (index < utf8.Length && lookup(utf8[index]))
                index++;
            return index;
            static bool lookup(int c) => c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
        }

        public static int Scan(ReadOnlySpan<char> chars)
        {
            var index = 0;
            while (index < chars.Length && lookup(chars[index]))
                index++;
            return index;
            static bool lookup(int c) => c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
        }

        public static bool Verify(ReadOnlySpan<byte> utf8)
        {
            var result = utf8.Length > 0 && Scan(utf8) == utf8.Length;
            return result;
        }

        public static bool Verify(ReadOnlySpan<char> chars)
        {
            var result = chars.Length > 0 && Scan(chars) == chars.Length;
            return result;
        }

        public static bool TryParse(ReadOnlySpan<byte> utf8, out RoomVerb verb)
        {
            if (Verify(utf8))
            {
                var data = new byte[utf8.Length];
                utf8.CopyTo(data);
                verb = new RoomVerb(data);
                return true;
            }
            else
            {
                verb = default;
                return false;
            }
        }

        public static bool TryParse(ReadOnlySpan<char> chars, out RoomVerb verb)
        {
            if (Verify(chars))
            {
                var data = new byte[Encoding.UTF8.GetByteCount(chars)];
                Encoding.UTF8.GetBytes(chars, data);
                verb = new RoomVerb(data);
                return true;
            }
            else
            {
                verb = default;
                return false;
            }
        }

        public static RoomVerb Parse(ReadOnlySpan<byte> utf8)
        {
            if (TryParse(utf8, out RoomVerb verb)) return verb;
            throw new FormatException($"Invalid verb format: {Encoding.UTF8.GetString(utf8)}");
        }

        public static RoomVerb Parse(ReadOnlySpan<char> chars)
        {
            if (TryParse(chars, out RoomVerb verb)) return verb;
            throw new FormatException($"Invalid verb format: {new string(chars)}");
        }

    }

}