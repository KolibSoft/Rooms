using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// Represents a sequence of 3 uppercase or lowercase letters of the ASCII code as UTF8 text.
    /// </summary>
    public struct RoomVerb
    {

        /// <summary>
        /// UTF8 internal data.
        /// </summary>
        public ArraySegment<byte> Data { get; }

        public int Length => Data.Count;

        /// <summary>
        /// Gets the string representation of the verb.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(Data);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (RoomVerb)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        /// <summary>
        /// Validate if the current data is a valid verb.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            var result = Verify(Data);
            return result;
        }

        /// <summary>
        /// Create a Verb with the utf8 data without validate it.
        /// </summary>
        /// <param name="utf8">UTF8 text.</param>
        public RoomVerb(ArraySegment<byte> utf8)
        {
            Data = utf8;
        }

        /// <summary>
        /// Verify if the provided UTF8 text is a valid verb.
        /// </summary>
        /// <param name="utf8">UTF8 text.</param>
        /// <returns></returns>
        public static bool Verify(ReadOnlySpan<byte> utf8)
        {
            if (utf8.Length != 3) return false;
            for (var i = 0; i < utf8.Length; i++)
            {
                var c = utf8[i];
                if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z')
                    continue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Verify if the provided string is a valid verb.
        /// </summary>
        /// <param name="string">String.</param>
        /// <returns></returns>
        public static bool Verify(ReadOnlySpan<char> @string)
        {
            if (@string.Length != 3) return false;
            for (var i = 0; i < @string.Length; i++)
            {
                var c = @string[i];
                if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z')
                    continue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses an UTF8 text into a verb.
        /// </summary>
        /// <param name="utf8">UTF8 text.</param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static RoomVerb Parse(ReadOnlySpan<byte> utf8)
        {
            if (!Verify(utf8))
                throw new FormatException($"Invalid verb format: {Encoding.UTF8.GetString(utf8)}");
            return new RoomVerb(utf8.ToArray());
        }

        /// <summary>
        /// Parses an string into a verb.
        /// </summary>
        /// <param name="string">String</param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static RoomVerb Parse(ReadOnlySpan<char> @string)
        {
            if (!Verify(@string))
                throw new FormatException($"Invalid verb format: {new string(@string)}");
            var utf8 = new byte[3];
            Encoding.UTF8.GetBytes(@string, utf8);
            return new RoomVerb(utf8);
        }

        public static bool operator ==(RoomVerb lhs, RoomVerb rhs)
        {
            return lhs.Data.SequenceEqual(rhs.Data);
        }

        public static bool operator !=(RoomVerb lhs, RoomVerb rhs)
        {
            return !lhs.Data.SequenceEqual(rhs.Data);
        }

        /// <summary>
        /// A default NNN verb.
        /// </summary>
        public static readonly RoomVerb None = Parse("NNN");

    }

}