using System.Text;

namespace KolibSoft.Rooms.Core;

public class RoomMessage
{

    public RoomVerb Verb { get; set; }
    public RoomChannel Channel { get; set; }
    public RoomContent Content { get; set; }

    public int Length => Verb.Data.Count + Channel.Data.Count + Content.Data.Count + 2;

    public void CopyTo(ArraySegment<byte> data)
    {
        Verb.Data.CopyTo(data.Slice(0, 3));
        Channel.Data.CopyTo(data.Slice(4, 8));
        Content.Data.CopyTo(data.Slice(13));
        data[3] = (byte)' ';
        data[12] = (byte)'\n';
    }

    public override string ToString()
    {
        return $"{Verb} {Channel}\n{Content}";
    }

    public RoomMessage(ArraySegment<byte> data)
    {
        Verb = new RoomVerb(data.Slice(0, 3));
        Channel = new RoomChannel(data.Slice(4, 8));
        Content = new RoomContent(data.Slice(13));
    }

    public RoomMessage()
    {
        Verb = RoomVerb.None;
        Channel = RoomChannel.Loopback;
        Content = RoomContent.None;
    }

    public static bool Verify(ReadOnlySpan<byte> utf8)
    {
        if (utf8.Length < 13) return false;
        var result = RoomVerb.Verify(utf8.Slice(0, 3)) && RoomChannel.Verify(utf8.Slice(4, 8)) && utf8[3] == ' ' && utf8[12] == '\n';
        return result;
    }

    public static bool Verify(ReadOnlySpan<char> @string)
    {
        if (@string.Length < 13) return false;
        var result = RoomVerb.Verify(@string.Slice(0, 3)) && RoomChannel.Verify(@string.Slice(4, 8)) && @string[3] == ' ' && @string[12] == '\n';
        return result;
    }

    public static RoomMessage Parse(ReadOnlySpan<byte> utf8)
    {
        if (!Verify(utf8))
            throw new FormatException($"Invalid message format: {Encoding.UTF8.GetString(utf8)}");
        var message = new RoomMessage(utf8.ToArray());
        return message;
    }

    public static RoomMessage Parse(ReadOnlySpan<char> @string)
    {
        if (!Verify(@string))
            throw new FormatException($"Invalid message format: {@string}");
        var utf8 = new byte[Encoding.UTF8.GetByteCount(@string)];
        Encoding.UTF8.GetBytes(@string, utf8);
        var message = new RoomMessage(utf8);
        return message;
    }

}