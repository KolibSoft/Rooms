namespace KolibSoft.Rooms.Core;

public class RoomMessage
{

    public RoomVerb Verb { get; set; } = RoomVerb.None;
    public RoomChannel Channel { get; set; } = RoomChannel.None;
    public RoomContent Content { get; set; } = RoomContent.None;

    public override string ToString()
    {
        return $"{Verb} {Channel}\n{Content}";
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