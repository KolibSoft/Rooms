using System.Text;
using KolibSoft.Rooms.Core.Protocol;

namespace Rooms.Tests;

public class RoomBufferTests
{

    [Theory]
    [InlineData("", "", "")]
    [InlineData("V", "0", "C")]
    [InlineData("DO", "15", "SOMETHING")]
    public void Write(string verb, string channel, string content)
    {
        var result = $"{verb} {channel} {Encoding.UTF8.GetByteCount(content)}\n{content}";
        var buffer = new RoomBuffer()
        {
            Verb = new RoomVerb(Encoding.UTF8.GetBytes(verb)),
            Channel = new RoomChannel(Encoding.UTF8.GetBytes(channel)),
            Content = RoomContent.Create(content)
        };
        Assert.Equal(buffer.ToString(), result);
    }

}