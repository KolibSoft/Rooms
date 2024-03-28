using System.Text;
using KolibSoft.Rooms.Core.Protocol;

namespace Rooms.Tests;

public class RoomLengthTests
{

    [Theory]
    [InlineData("", 0)]
    [InlineData("1234", 4)]
    [InlineData("1234VERB", 4)]
    [InlineData("VERB", 0)]
    [InlineData("VERB1234", 0)]
    public void Scan(string text, int result)
    {
        Assert.Equal(RoomLength.Scan(Encoding.UTF8.GetBytes(text)), result);
        Assert.Equal(RoomLength.Scan(text), result);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("1234", true)]
    [InlineData("1234VERB", false)]
    [InlineData("VERB", false)]
    [InlineData("VERB1234", false)]
    public void Parse(string text, bool result)
    {
        RoomLength length = default;
        Assert.Equal(RoomLength.TryParse(Encoding.UTF8.GetBytes(text), out length), result);
        Assert.Equal(RoomLength.TryParse(text, out length), result);
    }

}