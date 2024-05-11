using System;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Socekts
{
    public static class BufferUtils
    {

        public static Span<byte> WriteValue(this Span<byte> buffer, in byte value)
        {
            buffer[0] = value;
            return buffer.Slice(1);
        }

        public static Span<byte> WriteVerb(this Span<byte> buffer, in RoomVerb verb)
        {
            verb.Data.Span.CopyTo(buffer);
            return buffer.Slice(verb.Data.Length);
        }

        public static Span<byte> WriteChannel(this Span<byte> buffer, in RoomChannel channel)
        {
            channel.Data.Span.CopyTo(buffer);
            return buffer.Slice(channel.Data.Length);
        }

        public static Span<byte> WriteCount(this Span<byte> buffer, in RoomCount count)
        {
            count.Data.Span.CopyTo(buffer);
            return buffer.Slice(count.Data.Length);
        }

        public static Span<byte> WriteContent(this Span<byte> buffer, in RoomContent content)
        {
            content.Data.Span.CopyTo(buffer);
            return buffer.Slice(content.Data.Length);
        }

        public static ReadOnlySpan<byte> ReadValue(this ReadOnlySpan<byte> buffer, out byte value)
        {
            value = buffer[0];
            return buffer.Slice(1);
        }

        public static ReadOnlySpan<byte> ReadVerb(this ReadOnlySpan<byte> buffer, out RoomVerb verb)
        {
            var index = RoomVerb.Scan(buffer);
            verb = new RoomVerb(buffer.Slice(0, index).ToArray());
            return buffer.Slice(index);
        }

        public static ReadOnlySpan<byte> ReadChannel(this ReadOnlySpan<byte> buffer, out RoomChannel channel)
        {
            var index = RoomChannel.Scan(buffer);
            channel = new RoomChannel(buffer.Slice(0, index).ToArray());
            return buffer.Slice(index);
        }

        public static ReadOnlySpan<byte> ReadCount(this ReadOnlySpan<byte> buffer, out RoomCount channel)
        {
            var index = RoomCount.Scan(buffer);
            channel = new RoomCount(buffer.Slice(0, index).ToArray());
            return buffer.Slice(index);
        }

        public static ReadOnlySpan<byte> ReadContent(this ReadOnlySpan<byte> buffer, out RoomContent content)
        {
            content = new RoomContent(buffer.ToArray());
            return buffer.Slice(buffer.Length);
        }

    }
}