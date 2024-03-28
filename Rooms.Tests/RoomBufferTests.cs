using System.Text;
using KolibSoft.Rooms.Core.Protocol;

namespace Rooms.Tests;

public class RoomBufferTests
{

    [Theory]
    [InlineData("V", "1", "0", "")]
    [InlineData("VERB", "12345678", "1", "A")]
    [InlineData("VERB", "12345678", "0", "")]
    public void Write(string verb, string channel, string length, string content)
    {
        var buffer = new RoomBuffer();
        buffer.Verb = new RoomVerb(Encoding.UTF8.GetBytes(verb));
        buffer.Channel = new RoomChannel(Encoding.UTF8.GetBytes(channel));
        buffer.Length = new RoomLength(Encoding.UTF8.GetBytes(length));
        buffer.Content = new RoomContent(Encoding.UTF8.GetBytes(content));
        Assert.Equal(buffer.ToString(), $"{verb} {channel} {length}\n{content}");
    }

    [Theory]
    [InlineData("V", "1", "0", "")]
    [InlineData("VERB", "12345678", "1", "A")]
    [InlineData("VERB", "12345678", "0", "")]
    public void Read(string verb, string channel, string length, string content)
    {
        var data = Encoding.UTF8.GetBytes($"{verb}     {channel}     {length}\n\n\n\n{content}");
        var buffer = new RoomBuffer();
        data.CopyTo(buffer.Buffer.AsSpan());
        Assert.Equal(buffer.Verb.ToString(), verb);
        Assert.Equal(buffer.Channel.ToString(), channel);
        Assert.Equal(buffer.Length.ToString(), length);
        Assert.Equal(buffer.Content.ToString(), content);
    }

}