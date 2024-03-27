using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// Represents an 8-digit hexadecimal number that represents a 32-bit unsigned integer.
    /// </summary>
    public readonly struct RoomChannel
    {

        /// <summary>
        /// UTF8 internal data.
        /// </summary>
        private readonly ArraySegment<byte> data;

        /// <summary>
        /// Gets the length of the channel in bytes.
        /// </summary>
        public int Length => data.Count;

        /// <summary>
        /// Gets the string representation of the channel.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(data);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (RoomChannel)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        /// <summary>
        /// Validate if the current data is a valid channel.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            var result = Verify(data);
            return result;
        }

        /// <summary>
        /// Copies the internal data to target.
        /// </summary>
        /// <param name="target"></param>
        public void CopyTo(Span<byte> target)
        {
            data.AsSpan().CopyTo(target);
        }

        /// <summary>
        /// Create a Channel with the utf8 data without validate it.
        /// </summary>
        /// <param name="utf8">UTF8 text.</param>
        public RoomChannel(ArraySegment<byte> utf8)
        {
            data = utf8;
        }

        /// <summary>
        /// Verify if the provided UTF8 text is a valid channel.
        /// </summary>
        /// <param name="utf8">UTF8 text.</param>
        /// <returns></returns>
        public static bool Verify(ReadOnlySpan<byte> utf8)
        {
            if (utf8.Length != 8) return false;
            for (var i = 0; i < utf8.Length; i++)
            {
                var c = utf8[i];
                if (c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f')
                    continue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Verify if the provided string is a valid channel.
        /// </summary>
        /// <param name="string">String.</param>
        /// <returns></returns>
        public static bool Verify(ReadOnlySpan<char> @string)
        {
            if (@string.Length != 8) return false;
            for (var i = 0; i < @string.Length; i++)
            {
                var c = @string[i];
                if (c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f')
                    continue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses an UTF8 text into a channel.
        /// </summary>
        /// <param name="utf8">UTF8 text.</param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static RoomChannel Parse(ReadOnlySpan<byte> utf8)
        {
            if (!Verify(utf8))
                throw new FormatException($"Invalid channel format: {Encoding.UTF8.GetString(utf8)}");
            return new RoomChannel(utf8.ToArray());
        }

        /// <summary>
        /// Parses an string into a channel.
        /// </summary>
        /// <param name="string">String.</param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static RoomChannel Parse(ReadOnlySpan<char> @string)
        {
            if (!Verify(@string))
                throw new FormatException($"Invalid channel format: {new string(@string)}");
            var utf8 = new byte[8];
            Encoding.UTF8.GetBytes(@string, utf8);
            return new RoomChannel(utf8);
        }

        public static bool operator ==(RoomChannel lhs, RoomChannel rhs)
        {
            return lhs.data.SequenceEqual(rhs.data) || (int)lhs == (int)rhs;
        }

        public static bool operator !=(RoomChannel lhs, RoomChannel rhs)
        {
            return !lhs.data.SequenceEqual(rhs.data) && (int)lhs != (int)rhs;
        }

        public static implicit operator int(RoomChannel channel)
        {
            var @string = channel.ToString();
            var @int = Convert.ToInt32(@string, 16);
            return @int;
        }

        public static implicit operator RoomChannel(int @int)
        {
            var @string = @int.ToString();
            var channel = Parse(@string);
            return channel;
        }

        /// <summary>
        /// Loopback channel 00000000.
        /// </summary>
        public static readonly RoomChannel Loopback = Parse("00000000");

        /// <summary>
        /// Broadcast channel ffffffff.
        /// </summary>
        public static readonly RoomChannel Broadcast = Parse("ffffffff");

    }

}