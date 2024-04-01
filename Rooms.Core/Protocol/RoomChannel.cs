using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    /// <summary>
    /// Represents a 8-digit hexadecimal value.
    /// </summary>
    public readonly struct RoomChannel
    {

        /// <summary>
        /// Internal data.
        /// </summary>
        private readonly ArraySegment<byte> data;

        /// <summary>
        /// Internal data.
        /// </summary>
        public ReadOnlySpan<byte> Data => data;

        /// <summary>
        /// Length in bytes.
        /// </summary>
        public int Length => data.Count;

        public override string ToString() => Encoding.UTF8.GetString(data);

        public override int GetHashCode() => data.GetHashCode();

        public override bool Equals(object? obj)
        {
            var result = (obj is RoomChannel channel) && this == channel;
            return result;
        }

        /// <summary>
        /// Verify if the current data is a valid channel data.
        /// </summary>
        /// <returns></returns>
        public bool Validate() => Verify(data);

        /// <summary>
        /// Constructs a new channel without validate its data.
        /// </summary>
        /// <param name="data">Channel data.</param>
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

        /// <summary>
        /// Checks if a sequence starts with a valid channel data.
        /// </summary>
        /// <param name="utf8">Sequence to check.</param>
        /// <returns>The length of the found channel data.</returns>
        public static int Scan(ReadOnlySpan<byte> utf8)
        {
            var index = 0;
            while (index < utf8.Length && lookup(utf8[index]))
                index++;
            return index == 8 ? 8 : 0;
            static bool lookup(int c) => c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';
        }

        /// <summary>
        /// Checks if a sequence starts with a valid channel data.
        /// </summary>
        /// <param name="chars">Sequence to check.</param>
        /// <returns>The length of the found channel data.</returns>
        public static int Scan(ReadOnlySpan<char> chars)
        {
            var index = 0;
            while (index < chars.Length && lookup(chars[index]))
                index++;
            return index == 8 ? 8 : 0;
            static bool lookup(int c) => c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';
        }

        /// <summary>
        /// Checks if a sequence is a valid channel data.
        /// </summary>
        /// <param name="utf8">Sequence to check.</param>
        /// <returns>True if is a valid channel data.</returns>
        public static bool Verify(ReadOnlySpan<byte> utf8)
        {
            var result = Scan(utf8) == utf8.Length;
            return result;
        }

        /// <summary>
        /// Checks if a sequence is a valid channel data.
        /// </summary>
        /// <param name="chars">Sequence to check.</param>
        /// <returns>True if is a valid channel data.</returns>
        public static bool Verify(ReadOnlySpan<char> chars)
        {
            var result = Scan(chars) == chars.Length;
            return result;
        }

        /// <summary>
        /// Try to parse a sequence into a channel.
        /// </summary>
        /// <param name="utf8">Sequence to parse.</param>
        /// <param name="channel">Channel representation.</param>
        /// <returns>True if parse success.</returns>
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

        /// <summary>
        /// Try to parse a sequence into a channel.
        /// </summary>
        /// <param name="chars">Sequence to parse.</param>
        /// <param name="channel">Channel representation.</param>
        /// <returns>True if parse success.</returns>
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

        /// <summary>
        /// Parse a sequence into a channel.
        /// </summary>
        /// <param name="utf8">Sequence to parse.</param>
        /// <returns>Channel representation.</returns>
        /// <exception cref="FormatException">If the sequence is an invalid channel data.</exception>
        public static RoomChannel Parse(ReadOnlySpan<byte> utf8)
        {
            if (TryParse(utf8, out RoomChannel channel)) return channel;
            throw new FormatException($"Invalid channel format: {Encoding.UTF8.GetString(utf8)}");
        }

        /// <summary>
        /// Parse a sequence into a channel.
        /// </summary>
        /// <param name="chars">Sequence to parse.</param>
        /// <returns>Channel representation.</returns>
        /// <exception cref="FormatException">If the sequence is an invalid channel data.</exception>
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
            var text = number.ToString("x8");
            var channel = Parse(text);
            return channel;
        }

        /// <summary>
        /// Loopback representation (00000000).
        /// </summary>
        public static readonly RoomChannel Loopback = RoomChannel.Parse("00000000");

        /// <summary>
        /// Loopback representation (ffffffff).
        /// </summary>
        public static readonly RoomChannel Broadcast = RoomChannel.Parse("ffffffff");

    }

}