using System.Text;
using KolibSoft.Rooms.Core.Protocol;

namespace Rooms.Tests;

public class RoomVerbTests
{

    [Theory]
    [InlineData("", 0)]
    [InlineData("0000", 0)]
    [InlineData("0000VERB", 0)]
    [InlineData("VERB", 4)]
    [InlineData("VERB000", 4)]
    public void Scan(string text, int result)
    {
        Assert.Equal(RoomVerb.Scan(Encoding.UTF8.GetBytes(text)), result);
        Assert.Equal(RoomVerb.Scan(text), result);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("0000", false)]
    [InlineData("0000VERB", false)]
    [InlineData("VERB", true)]
    [InlineData("VERB000", false)]
    public void Parse(string text, bool result)
    {
        RoomVerb verb = default;
        Assert.Equal(RoomVerb.TryParse(Encoding.UTF8.GetBytes(text), out verb), result);
        Assert.Equal(RoomVerb.TryParse(text, out verb), result);
    }

}