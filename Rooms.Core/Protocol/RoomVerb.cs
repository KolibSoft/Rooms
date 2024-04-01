using System;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    /// <summary>
    /// Represents a variable length verb value.
    /// </summary>
    public readonly struct RoomVerb
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
            var result = (obj is RoomVerb verb) && this == verb;
            return result;
        }

        /// <summary>
        /// Verify if the current data is a valid verb data.
        /// </summary>
        /// <returns></returns>
        public bool Validate() => Verify(data);

        /// <summary>
        /// Constructs a new verb without validate its data.
        /// </summary>
        /// <param name="data">Verb data.</param>
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

        /// <summary>
        /// Checks if a sequence starts with a valid verb data.
        /// </summary>
        /// <param name="utf8">Sequence to check.</param>
        /// <returns>The length of the found verb data.</returns>
        public static int Scan(ReadOnlySpan<byte> utf8)
        {
            var index = 0;
            while (index < utf8.Length && lookup(utf8[index]))
                index++;
            return index;
            static bool lookup(int c) => c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
        }

        /// <summary>
        /// Checks if a sequence starts with a valid verb data.
        /// </summary>
        /// <param name="chars">Sequence to check.</param>
        /// <returns>The length of the found verb data.</returns>
        public static int Scan(ReadOnlySpan<char> chars)
        {
            var index = 0;
            while (index < chars.Length && lookup(chars[index]))
                index++;
            return index;
            static bool lookup(int c) => c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
        }

        /// <summary>
        /// Checks if a sequence is a valid verb data.
        /// </summary>
        /// <param name="utf8">Sequence to check.</param>
        /// <returns>True if is a valid verb data.</returns>
        public static bool Verify(ReadOnlySpan<byte> utf8)
        {
            var result = Scan(utf8) == utf8.Length;
            return result;
        }

        /// <summary>
        /// Checks if a sequence is a valid verb data.
        /// </summary>
        /// <param name="chars">Sequence to check.</param>
        /// <returns>True if is a valid verb data.</returns>
        public static bool Verify(ReadOnlySpan<char> chars)
        {
            var result = Scan(chars) == chars.Length;
            return result;
        }

        /// <summary>
        /// Try to parse a sequence into a verb.
        /// </summary>
        /// <param name="utf8">Sequence to parse.</param>
        /// <param name="verb">Verb representation.</param>
        /// <returns>True if parse success.</returns>
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

        /// <summary>
        /// Try to parse a sequence into a verb.
        /// </summary>
        /// <param name="chars">Sequence to parse.</param>
        /// <param name="verb">Verb representation.</param>
        /// <returns>True if parse success.</returns>
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

        /// <summary>
        /// Parse a sequence into a verb.
        /// </summary>
        /// <param name="utf8">Sequence to parse.</param>
        /// <returns>Verb representation.</returns>
        /// <exception cref="FormatException">If the sequence is an invalid verb data.</exception>
        public static RoomVerb Parse(ReadOnlySpan<byte> utf8)
        {
            if (TryParse(utf8, out RoomVerb verb)) return verb;
            throw new FormatException($"Invalid verb format: {Encoding.UTF8.GetString(utf8)}");
        }

        /// <summary>
        /// Parse a sequence into a verb.
        /// </summary>
        /// <param name="chars">Sequence to parse.</param>
        /// <returns>Verb representation.</returns>
        /// <exception cref="FormatException">If the sequence is an invalid verb data.</exception>
        public static RoomVerb Parse(ReadOnlySpan<char> chars)
        {
            if (TryParse(chars, out RoomVerb verb)) return verb;
            throw new FormatException($"Invalid verb format: {new string(chars)}");
        }

    }

}