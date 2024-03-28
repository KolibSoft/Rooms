using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomChannel
    {

        private readonly ArraySegment<byte> data;
        
        public ReadOnlyMemory<byte> Data => data;

        public int Length => data.Count;

        public override string ToString() => Encoding.UTF8.GetString(data);

        public override int GetHashCode() => data.GetHashCode();

        public override bool Equals(object? obj)
        {
            var result = (obj is RoomChannel channel) && this == channel;
            return result;
        }

        public void CopyTo(Span<byte> target)
        {
            if (target.Length < data.Count) throw new ArgumentException("Target is too short");
            data.AsSpan().CopyTo(target);
        }

        public RoomChannel(ArraySegment<byte> data)
        {
            this.data = data;
        }

        public static bool operator ==(RoomChannel lhs, RoomChannel rhs)
        {
            var result = lhs.data.SequenceEqual(rhs.data);
            return result;
        }

        public static bool operator !=(RoomChannel lhs, RoomChannel rhs)
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
            static bool lookup(int c) => c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';
        }

        public static int Scan(ReadOnlySpan<char> chars)
        {
            var index = 0;
            while (index < chars.Length && lookup(chars[index]))
                index++;
            return index;
            static bool lookup(int c) => c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';
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

        public static bool TryParse(ReadOnlySpan<byte> utf8, out RoomChannel channel)
        {
            if (Verify(utf8))
            {
                var data = new byte[utf8.Length];
                utf8.CopyTo(data);
                channel = new RoomChannel(data);
                return true;
            }
            else
            {
                channel = default;
                return false;
            }
        }

        public static bool TryParse(ReadOnlySpan<char> chars, out RoomChannel channel)
        {
            if (Verify(chars))
            {
                var data = new byte[Encoding.UTF8.GetByteCount(chars)];
                Encoding.UTF8.GetBytes(chars, data);
                channel = new RoomChannel(data);
                return true;
            }
            else
            {
                channel = default;
                return false;
            }
        }

        public static RoomChannel Parse(ReadOnlySpan<byte> utf8)
        {
            if (TryParse(utf8, out RoomChannel channel)) return channel;
            throw new FormatException($"Invalid channel format: {Encoding.UTF8.GetString(utf8)}");
        }

        public static RoomChannel Parse(ReadOnlySpan<char> chars)
        {
            if (TryParse(chars, out RoomChannel channel)) return channel;
            throw new FormatException($"Invalid channel format: {new string(chars)}");
        }

        public static implicit operator int(RoomChannel channel)
        {
            var text = Encoding.UTF8.GetString(channel.data);
            var number = int.Parse(text, NumberStyles.HexNumber);
            return number;
        }

        public static implicit operator RoomChannel(int number)
        {
            var text = number.ToString("x");
            var channel = Parse(text);
            return channel;
        }

    }

}