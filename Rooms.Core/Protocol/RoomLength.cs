using System;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomLength
    {

        private readonly ArraySegment<byte> data;

        public override string ToString() => Encoding.UTF8.GetString(data);

        public override int GetHashCode() => data.GetHashCode();

        public override bool Equals(object? obj)
        {
            var result = (obj is RoomLength length) && this == length;
            return result;
        }

        public RoomLength(ArraySegment<byte> data)
        {
            this.data = data;
        }

        public static bool operator ==(RoomLength lhs, RoomLength rhs)
        {
            var result = lhs.data.SequenceEqual(rhs.data);
            return result;
        }

        public static bool operator !=(RoomLength lhs, RoomLength rhs)
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
            static bool lookup(int c) => c >= '0' && c <= '9';
        }

        public static int Scan(ReadOnlySpan<char> chars)
        {
            var index = 0;
            while (index < chars.Length && lookup(chars[index]))
                index++;
            return index;
            static bool lookup(int c) => c >= '0' && c <= '9';
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

        public static bool TryParse(ReadOnlySpan<byte> utf8, out RoomLength length)
        {
            if (Verify(utf8))
            {
                var data = new byte[utf8.Length];
                utf8.CopyTo(data);
                length = new RoomLength(data);
                return true;
            }
            else
            {
                length = default;
                return false;
            }
        }

        public static bool TryParse(ReadOnlySpan<char> chars, out RoomLength length)
        {
            if (Verify(chars))
            {
                var data = new byte[Encoding.UTF8.GetByteCount(chars)];
                Encoding.UTF8.GetBytes(chars, data);
                length = new RoomLength(data);
                return true;
            }
            else
            {
                length = default;
                return false;
            }
        }

        public static RoomLength Parse(ReadOnlySpan<byte> utf8)
        {
            if (TryParse(utf8, out RoomLength length)) return length;
            throw new FormatException($"Invalid length format: {Encoding.UTF8.GetString(utf8)}");
        }

        public static RoomLength Parse(ReadOnlySpan<char> chars)
        {
            if (TryParse(chars, out RoomLength length)) return length;
            throw new FormatException($"Invalid length format: {new string(chars)}");
        }

    }

}