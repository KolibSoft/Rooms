using System.Text;
using KolibSoft.Rooms.Core.Protocol;

namespace Rooms.Tests;

public class RoomBufferTests
{

    [Theory]
    [InlineData("VERB", "12345678", "1", "A")]
    public void Write(string verb, string channel, string length, string content)
    {
        var buffer = new RoomBuffer();
        buffer.Verb = new RoomVerb(Encoding.UTF8.GetBytes(verb));
        buffer.Channel = new RoomChannel(Encoding.UTF8.GetBytes(channel));
        buffer.Length = new RoomLength(Encoding.UTF8.GetBytes(length));
        buffer.Content = new RoomContent(Encoding.UTF8.GetBytes(content));
        Assert.Equal(buffer.ToString(), $"{verb}{channel}{length}{content}");
    }

}