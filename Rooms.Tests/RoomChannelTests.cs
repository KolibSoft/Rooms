using System.Text;
using KolibSoft.Rooms.Core.Protocol;

namespace Rooms.Tests;

public class RoomChannelTests
{

    [Theory]
    [InlineData("", 0)]
    [InlineData("09AFaf", 6)]
    [InlineData("09AFafVERB", 6)]
    [InlineData("VERB", 0)]
    [InlineData("VERB09AFaf", 0)]
    public void Scan(string text, int result)
    {
        Assert.Equal(RoomChannel.Scan(Encoding.UTF8.GetBytes(text)), result);
        Assert.Equal(RoomChannel.Scan(text), result);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("09AFaf", true)]
    [InlineData("09AFafVERB", false)]
    [InlineData("VERB", false)]
    [InlineData("VERB09AFaf", false)]
    public void Parse(string text, bool result)
    {
        RoomChannel channel = default;
        Assert.Equal(RoomChannel.TryParse(Encoding.UTF8.GetBytes(text), out channel), result);
        Assert.Equal(RoomChannel.TryParse(text, out channel), result);
    }

}