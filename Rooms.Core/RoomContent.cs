using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// Represents a variable length UTF8 text.
    /// </summary>
    public readonly struct RoomContent
    {

        /// <summary>
        /// UTF8 internal data.
        /// </summary>
        private readonly ArraySegment<byte> data;

        /// <summary>
        /// Gets the length of the content in bytes.
        /// </summary>
        public int Length => data.Count;

        /// <summary>
        /// Gets the string representation of the content.
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

            var other = (RoomContent)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        /// <summary>
        /// Create a Content with the utf8 data without validate it.
        /// </summary>
        /// <param name="utf8">UTF8 text.</param>
        private RoomContent(ArraySegment<byte> utf8)
        {
            data = utf8;
        }

        /// <summary>
        /// Parses an UTF8 text into a content.
        /// </summary>
        /// <param name="utf8">UTF8 text.</param>
        /// <returns></returns>
        public static RoomContent Parse(ReadOnlySpan<byte> utf8)
        {
            return new RoomContent(utf8.ToArray());
        }

        /// <summary>
        /// Parses an string into a content.
        /// </summary>
        /// <param name="string">String.</param>
        /// <returns></returns>
        public static RoomContent Parse(ReadOnlySpan<char> @string)
        {
            var utf8 = new byte[Encoding.UTF8.GetByteCount(@string)];
            Encoding.UTF8.GetBytes(@string, utf8);
            return new RoomContent(utf8);
        }

        public static bool operator ==(RoomContent lhs, RoomContent rhs)
        {
            return lhs.data.SequenceEqual(rhs.data);
        }

        public static bool operator !=(RoomContent lhs, RoomContent rhs)
        {
            return !lhs.data.SequenceEqual(rhs.data);
        }

        /// <summary>
        /// A default empty content.
        /// </summary>
        public static readonly RoomContent None = Parse("");

    }

}