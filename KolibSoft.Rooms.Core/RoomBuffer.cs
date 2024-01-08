namespace KolibSoft.Rooms.Core;

public class RoomBuffer
{

    private ArraySegment<byte> buffer;
    private ArraySegment<byte> verb;
    private ArraySegment<byte> channel;
    private ArraySegment<byte> content;

    public RoomVerb Verb
    {
        get => new RoomVerb(verb);
        set => value.Data.CopyTo(verb);
    }

    public RoomChannel Channel
    {
        get => new RoomChannel(channel);
        set => value.Data.CopyTo(channel);
    }

    public RoomContent Content
    {
        get => new RoomContent(content);
        set
        {
            var chunk = buffer.Slice(12);
            value.Data.CopyTo(chunk);
            content = chunk.Slice(0, int.Min(value.Data.Count, chunk.Count));
        }
    }

    public ArraySegment<byte> Data => buffer.Slice(0, verb.Count + channel.Count + content.Count + 2);

    public void SetMessage(RoomMessage message)
    {
        Verb = message.Verb;
        Channel = message.Channel;
        Content = message.Content;
    }

    public RoomMessage GetMessage()
    {
        var message = new RoomMessage();
        message.Verb = new RoomVerb(verb.ToArray());
        message.Channel = new RoomChannel(channel.ToArray());
        message.Content = new RoomContent(content.ToArray());
        return message;
    }

    public override string ToString()
    {
        return $"{Verb} {Channel}\n{Content}";
    }

    public RoomBuffer(ArraySegment<byte> buffer)
    {
        this.buffer = buffer;
        verb = buffer.Slice(0, 3);
        channel = buffer.Slice(4, 8);
        content = buffer.Slice(13);
        buffer[3] = (byte)' ';
        buffer[12] = (byte)'\n';
    }

    public RoomBuffer(int bufferSize = 1024) : this(new byte[bufferSize])
    {
        Verb = RoomVerb.None;
        Channel = RoomChannel.None;
        Content = RoomContent.None;
    }

}