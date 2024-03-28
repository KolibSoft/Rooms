using System;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomContent
    {

        internal readonly ArraySegment<byte> data;

        public int Length => data.Count;

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

        public static int Scan(ReadOnlySpan<byte> utf8)
        {
            var index = 0;
            while (index < utf8.Length && lookup(utf8[index]))
                index++;
            if (index > 0)
            {
                var length = int.Parse(Encoding.UTF8.GetString(utf8[..index]));
                if (utf8.Length < (index + length + 1) || utf8[index] != '\n') return 0;
                index += length + 1;
            }
            return index;
            static bool lookup(int c) => c >= '0' && c <= '9';
        }

        public static int Scan(ReadOnlySpan<char> chars)
        {
            var index = 0;
            while (index < chars.Length && lookup(chars[index]))
                index++;
            if (index > 0)
            {
                var length = int.Parse(chars[..index]);
                if (chars.Length < (index + length + 1) || chars[index] != '\n') return 0;
                index += length + 1;
            }
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

        public static bool TryParse(ReadOnlySpan<byte> utf8, out RoomContent content)
        {
            if (Verify(utf8))
            {
                var data = new byte[utf8.Length];
                utf8.CopyTo(data);
                content = new RoomContent(data);
                return true;
            }
            else
            {
                content = default;
                return false;
            }
        }

        public static bool TryParse(ReadOnlySpan<char> chars, out RoomContent content)
        {
            if (Verify(chars))
            {
                var data = new byte[Encoding.UTF8.GetByteCount(chars)];
                Encoding.UTF8.GetBytes(chars, data);
                content = new RoomContent(data);
                return true;
            }
            else
            {
                content = default;
                return false;
            }
        }

        public static RoomContent Parse(ReadOnlySpan<byte> utf8)
        {
            if (TryParse(utf8, out RoomContent content)) return content;
            throw new FormatException($"Invalid content format: {Encoding.UTF8.GetString(utf8)}");
        }

        public static RoomContent Parse(ReadOnlySpan<char> chars)
        {
            if (TryParse(chars, out RoomContent content)) return content;
            throw new FormatException($"Invalid content format: {new string(chars)}");
        }

        public static RoomContent Create(ReadOnlySpan<byte> utf8)
        {
            var length = $"{utf8.Length}\n";
            var length_count = Encoding.UTF8.GetByteCount(length);
            var data = new byte[length_count + utf8.Length];
            Encoding.UTF8.GetBytes(length, data);
            utf8.CopyTo(data.AsSpan()[length_count..]);
            var content = new RoomContent(data);
            return content;
        }

        public static RoomContent Create(ReadOnlySpan<char> chars)
        {
            var length = $"{chars.Length}\n";
            var length_count = Encoding.UTF8.GetByteCount(length);
            var chars_count = Encoding.UTF8.GetByteCount(chars);
            var data = new byte[length_count + chars_count];
            Encoding.UTF8.GetBytes(length, data);
            Encoding.UTF8.GetBytes(chars, data.AsSpan()[length_count..]);
            var content = new RoomContent(data);
            return content;
        }

    }

}