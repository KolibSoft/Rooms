namespace KolibSoft.Rooms.Web;

public class Message
{

    public Dictionary<string, string> Headers { get; } = new();
    public string Body { get; set; } = string.Empty;

    public override string ToString()
    {
        var @string = string.Join('\n', Headers.Select(x => $"{x.Key}:{x.Value}")) + $"\n\n{Body}";
        return @string;
    }

    public static Message Parse(string @string)
    {
        var message = new Message();
        var message_split = @string.Split("\n\n");
        if (message_split.Length != 2)
            throw new FormatException("Message body and headers are required");
        if (message_split[0].Length > 0)
            foreach (var header in message_split[0].Split("\n"))
            {
                var header_split = header.Split(":");
                if (header_split.Length != 2)
                    throw new FormatException("Header name and value are required");
                message.Headers[header_split[0].Trim()] = header_split[1].Trim();
            }
        message.Body = message_split[1];
        return message;
    }

}