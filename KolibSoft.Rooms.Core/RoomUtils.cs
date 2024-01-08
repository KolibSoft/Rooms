using System.Text;

namespace KolibSoft.Rooms.Core;

public static class RoomUtils
{

    public static RoomMessage ToMessage(this ArraySegment<byte> segment)
    {
        var message = new RoomMessage()
        {
            Verb = new RoomVerb(segment.Slice(0, 3)),
            Channel = new RoomChannel(segment.Slice(4, 8)),
            Content = new RoomContent(segment.Slice(13))
        };
        return message;
    }

    public static RoomMessage ToMessage(this byte[] bytes)
    {
        var segment = new ArraySegment<byte>(bytes);
        return segment.ToMessage();
    }

    public static RoomMessage ToMessage(this string @string)
    {
        var bytes = Encoding.UTF8.GetBytes(@string);
        return bytes.ToMessage();
    }

}