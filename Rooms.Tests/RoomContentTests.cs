using System.Text;
using KolibSoft.Rooms.Core.Protocol;

namespace Rooms.Tests;

public class RoomContentTests
{

    [Theory]
    [InlineData("", 0)]
    [InlineData("123 Content", 0)]
    [InlineData("1\nA", 3)]
    [InlineData("7 Content", 0)]
    [InlineData("7\nContent", 9)]
    public void Scan(string text, int result)
    {
        Assert.Equal(RoomContent.Scan(Encoding.UTF8.GetBytes(text)), result);
        Assert.Equal(RoomContent.Scan(text), result);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("123 Content", false)]
    [InlineData("1\nA", true)]
    [InlineData("7 Content", false)]
    [InlineData("7\nContent", true)]
    public void Parse(string text, bool result)
    {
        RoomContent content = default;
        Assert.Equal(RoomContent.TryParse(Encoding.UTF8.GetBytes(text), out content), result);
        Assert.Equal(RoomContent.TryParse(text, out content), result);
    }

    [Theory]
    [InlineData("Content Example", "15\nContent Example")]
    public void Create(string text, string result)
    {
        var content = RoomContent.Create(text);
        Assert.Equal(content.ToString(), result);
    }

}