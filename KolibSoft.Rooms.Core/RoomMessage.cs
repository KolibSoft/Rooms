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
        Channel = RoomChannel.None;
        Content = RoomContent.None;
    }

    public static RoomMessage Parse(string @string)
    {
        if (@string.Length < 13 || @string[3] != ' ' || @string[12] != '\n')
            throw new FormatException();
        var message = new RoomMessage();
        message.Verb = RoomVerb.Parse(@string.Substring(0, 3));
        message.Channel = RoomChannel.Parse(@string.Substring(4, 8));
        message.Content = RoomContent.Parse(@string.Substring(13));
        return message;
    }

}