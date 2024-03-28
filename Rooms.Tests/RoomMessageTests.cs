using System.Text;
using KolibSoft.Rooms.Core.Protocol;

namespace Rooms.Tests;

public class RoomMessageTests
{

    [Theory]
    [InlineData("V", "0", "")]
    [InlineData("VERB", "1234", "SOME CONTENT")]
    public void CopyTo(string verb, string channel, string content)
    {
        var text = $"{verb} {channel}\n{content}";
        var message = new RoomMessage
        {
            Verb = RoomVerb.Parse(verb),
            Channel = RoomChannel.Parse(channel),
            Content = RoomContent.Parse(content)
        };
        var buffer = new byte[message.Length];
        message.CopyTo(buffer);
        Assert.Equal(message.ToString(), text);
    }

    [Theory]
    [InlineData("V", "0", "")]
    [InlineData("VERB", "1234", "SOME CONTENT")]
    public void CopyFrom(string verb, string channel, string content)
    {
        var text = $"{verb} {channel}\n{content}";
        var buffer = Encoding.UTF8.GetBytes(text);
        var message = new RoomMessage();
        message.CopyFrom(buffer);
        Assert.Equal(message.ToString(), text);
    }

}