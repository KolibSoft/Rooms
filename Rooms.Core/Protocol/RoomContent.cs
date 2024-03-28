using System;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    /// <summary>
    /// Represents a variable length verb value.
    /// </summary>
    public readonly struct RoomContent
    {

        /// <summary>
        /// Internal data.
        /// </summary>
        private readonly ArraySegment<byte> data;

        /// <summary>
        /// Internal data.
        /// </summary>
        public ReadOnlyMemory<byte> Data => data;

        /// <summary>
        /// Length in bytes.
        /// </summary>
        public int Length => data.Count;

        public override string ToString() => Encoding.UTF8.GetString(data);

        public override int GetHashCode() => data.GetHashCode();

        public override bool Equals(object? obj)
        {
            var result = (obj is RoomContent content) && this == content;
            return result;
        }

        /// <summary>
        /// Copies the content into another buffer.
        /// </summary>
        /// <param name="target">Buffer to write.</param>
        /// <exception cref="ArgumentException">If target is too short.</exception>
        public void CopyTo(Span<byte> target)
        {
            if (target.Length < data.Count) throw new ArgumentException("Target is too short");
            data.AsSpan().CopyTo(target);
        }

        /// <summary>
        /// Constructs a new content without validate its data.
        /// </summary>
        /// <param name="data">Content data.</param>
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

        /// <summary>
        /// Create a new content from UTF8 text.
        /// </summary>
        /// <param name="utf8">UTF8 text data.</param>
        /// <returns>Content representation.</returns>
        public static RoomContent Create(ReadOnlySpan<byte> utf8)
        {
            var data = new byte[utf8.Length];
            utf8.CopyTo(data);
            var content = new RoomContent(data);
            return content;
        }

        /// <summary>
        /// Create a new content from UTF16 text.
        /// </summary>
        /// <param name="chars">UTF16 text data.</param>
        /// <returns>Content representation.</returns>
        public static RoomContent Create(ReadOnlySpan<char> chars)
        {
            var data = new byte[Encoding.UTF8.GetByteCount(chars)];
            Encoding.UTF8.GetBytes(chars, data);
            var content = new RoomContent(data);
            return content;
        }

    }

}