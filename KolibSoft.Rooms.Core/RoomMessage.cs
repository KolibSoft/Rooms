namespace KolibSoft.Rooms.Core;

public class RoomMessage
{

    public RoomVerb Verb { get; set; } = RoomVerb.None;
    public RoomIdentifier Identifier { get; set; } = RoomIdentifier.None;
    public RoomContent Content { get; set; } = RoomContent.None;

    public override string ToString()
    {
        return $"{Verb} {Identifier}\n{Content}";
    }

    public static RoomMessage Parse(string @string)
    {
        if (@string.Length < 13 || @string[3] != ' ' || @string[12] != '\n')
            throw new FormatException();
        var message = new RoomMessage();
        message.Verb = RoomVerb.Parse(@string.Substring(0, 3));
        message.Identifier = RoomIdentifier.Parse(@string.Substring(4, 8));
        message.Content = RoomContent.Parse(@string.Substring(13));
        return message;
    }

}