using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    /// <summary>
    /// Representas a message with verb, channel and content.
    /// </summary>
    public class RoomMessage
    {

        /// <summary>
        /// Verb value.
        /// </summary>
        public RoomVerb Verb { get; set; }

        /// <summary>
        /// Channel value.
        /// </summary>
        public RoomChannel Channel { get; set; }

        /// <summary>
        /// Content value.
        /// </summary>
        public RoomContent Content { get; set; }

        /// <summary>
        /// Length in bytes.
        /// </summary>
        public int Length => Verb.Length + Channel.Length + 1 + (Content.Length > 0 ? Content.Length + 1 : 0);

        public override string ToString() => $"{Verb} {Channel} {Content}";

        /// <summary>
        /// Copies the content into another buffer.
        /// </summary>
        /// <param name="target">Buffer to write.</param>
        /// <exception cref="ArgumentException">If target is too short.</exception>
        public void CopyTo(Span<byte> target)
        {
            if (target.Length < Length) throw new ArgumentException("Target is too short");
            var offset = 0;
            Verb.CopyTo(target[offset..]);
            offset += Verb.Length;
            target[offset] = (byte)' ';
            offset += 1;
            Channel.CopyTo(target[offset..]);
            offset += Channel.Length;
            if (Content.Length > 0)
            {
                target[offset] = (byte)' ';
                Content.CopyTo(target[(offset + 1)..]);
            }
        }

        /// <summary>
        /// Copies the data from another buffer.
        /// </summary>
        /// <param name="source">Buffer to read.</param>
        public void CopyFrom(ReadOnlySpan<byte> source)
        {
            var offset = 0;
            var length = RoomVerb.Scan(source[offset..]);
            Verb = new RoomVerb(source[offset..(offset + length)].ToArray());
            offset += Verb.Length + 1;
            length = RoomChannel.Scan(source[offset..]);
            Channel = new RoomChannel(source[offset..(offset + length)].ToArray());
            offset += Channel.Length;
            if (offset < source.Length) Content = new RoomContent(source[(offset + 1)..].ToArray());
        }

        /// <summary>
        /// Constructs a new message without validate its data.
        /// </summary>
        /// <param name="data">Message data.</param>
        public RoomMessage(ArraySegment<byte> data)
        {
            var offset = 0;
            var length = RoomVerb.Scan(data[offset..]);
            Verb = new RoomVerb(data[offset..(offset + length)]);
            offset += Verb.Length + 1;
            length = RoomChannel.Scan(data[offset..]);
            Channel = new RoomChannel(data[offset..(offset + length)]);
            offset += Channel.Length;
            if (offset < data.Count) Content = new RoomContent(data[(offset + 1)..]);
        }

        /// <summary>
        /// Constructs an empty message.
        /// </summary>
        public RoomMessage() { }

        /// <summary>
        /// Checks if a sequence is a valid message data.
        /// </summary>
        /// <param name="utf8">Sequence to check.</param>
        /// <returns>True if is a valid message data.</returns>
        public static bool Verify(ReadOnlySpan<byte> utf8)
        {
            var offset = 0;
            var index = RoomVerb.Scan(utf8);
            if (index == 0 || index == utf8.Length) return false;
            offset += index;
            if (!char.IsWhiteSpace((char)utf8[offset])) return false;
            offset++;
            index = RoomChannel.Scan(utf8[offset..]);
            if (index == 0 || index == utf8.Length) return false;
            offset += index;
            if (offset < utf8.Length && !char.IsWhiteSpace((char)utf8[offset])) return false;
            return true;
        }

        /// <summary>
        /// Checks if a sequence is a valid message data.
        /// </summary>
        /// <param name="chars">Sequence to check.</param>
        /// <returns>True if is a valid message data.</returns>
        public static bool Verify(ReadOnlySpan<char> chars)
        {
            var offset = 0;
            var index = RoomVerb.Scan(chars);
            if (index == 0 || index == chars.Length) return false;
            offset += index;
            if (!char.IsWhiteSpace(chars[offset])) return false;
            offset++;
            index = RoomChannel.Scan(chars[offset..]);
            if (index == 0 || index == chars.Length) return false;
            offset += index;
            if (offset < chars.Length && !char.IsWhiteSpace(chars[offset])) return false;
            return true;
        }

        /// <summary>
        /// Try to parse a sequence into a message.
        /// </summary>
        /// <param name="utf8">Sequence to parse.</param>
        /// <param name="message">Message representation.</param>
        /// <returns>True if parse success.</returns>
        public static bool TryParse(ReadOnlySpan<byte> utf8, [NotNullWhen(true)] out RoomMessage? message)
        {
            if (Verify(utf8))
            {
                var data = new byte[utf8.Length];
                utf8.CopyTo(data);
                message = new RoomMessage(data);
                return true;
            }
            else
            {
                message = null;
                return false;
            }
        }

        /// <summary>
        /// Try to parse a sequence into a message.
        /// </summary>
        /// <param name="chars">Sequence to parse.</param>
        /// <param name="message">Message representation.</param>
        /// <returns>True if parse success.</returns>
        public static bool TryParse(ReadOnlySpan<char> chars, [NotNullWhen(true)] out RoomMessage? message)
        {
            if (Verify(chars))
            {
                var data = new byte[Encoding.UTF8.GetByteCount(chars)];
                Encoding.UTF8.GetBytes(chars, data);
                message = new RoomMessage(data);
                return true;
            }
            else
            {
                message = null;
                return false;
            }
        }

        /// <summary>
        /// Parse a sequence into a message.
        /// </summary>
        /// <param name="utf8">Sequence to parse.</param>
        /// <returns>Message representation.</returns>
        /// <exception cref="FormatException">If the sequence is an invalid message data.</exception>
        public static RoomMessage Parse(ReadOnlySpan<byte> utf8)
        {
            if (TryParse(utf8, out RoomMessage? message)) return message;
            throw new FormatException($"Invalid message format: {Encoding.UTF8.GetString(utf8)}");
        }

        /// <summary>
        /// Parse a sequence into a message.
        /// </summary>
        /// <param name="chars">Sequence to parse.</param>
        /// <returns>Message representation.</returns>
        /// <exception cref="FormatException">If the sequence is an invalid message data.</exception>
        public static RoomMessage Parse(ReadOnlySpan<char> chars)
        {
            if (TryParse(chars, out RoomMessage? message)) return message;
            throw new FormatException($"Invalid message format: {new string(chars)}");
        }

    }

}