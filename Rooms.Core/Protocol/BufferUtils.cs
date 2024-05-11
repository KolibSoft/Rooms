using System;

namespace KolibSoft.Rooms.Core.Protocol
{
    public static class BufferUtils
    {
        public static void ReadRoomHead(this ReadOnlySpan<byte> buffer, ref RoomVerb verb, ref RoomChannel channel, ref RoomCount count)
        {
            var offset = 0;
            var length = 0;
            length = RoomVerb.Scan(buffer.Slice(offset));
            if (length == 0) throw new FormatException("Wrong Room Verb");
            var verbSlice = buffer.Slice(offset, length);
            offset += length;
            if (buffer[offset] != ' ') throw new FormatException("Wrong white space");
            offset++;
            length = RoomChannel.Scan(buffer.Slice(offset));
            if (length == 0) throw new FormatException("Wrong Room Channel");
            var channelSlice = buffer.Slice(offset, length);
            offset += length;
            if (buffer[offset] != ' ') throw new FormatException("Wrong white space");
            offset++;
            length = RoomCount.Scan(buffer.Slice(offset));
            if (length == 0) throw new FormatException("Wrong Room Count");
            var countSlice = buffer.Slice(offset, length);
            offset += length;
            if (buffer[offset] != '\n') throw new FormatException("Wrong line break");
            offset++;
            verb = new RoomVerb(verbSlice.ToArray());
            channel = new RoomChannel(channelSlice.ToArray());
            count = new RoomCount(countSlice.ToArray());
        }

        public static void WriteRoomHead(this Span<byte> buffer, ref RoomVerb verb, ref RoomChannel channel, ref RoomCount count)
        {
            var offset = 0;
            if (!RoomVerb.Verify(verb.Data.Span)) throw new FormatException("Wrong Room Verb");
            verb.Data.Span.CopyTo(buffer.Slice(offset));
            offset += verb.Data.Length;
            buffer[offset] = (byte)' ';
            offset++;
            if (!RoomChannel.Verify(channel.Data.Span)) throw new FormatException("Wrong Room Channel");
            channel.Data.Span.CopyTo(buffer.Slice(offset));
            offset += channel.Data.Length;
            buffer[offset] = (byte)' ';
            offset++;
            if (!RoomCount.Verify(count.Data.Span)) throw new FormatException("Wrong Room Count");
            count.Data.Span.CopyTo(buffer.Slice(offset));
            offset += count.Data.Length;
            buffer[offset] = (byte)'\n';
            offset++;
        }

    }
}