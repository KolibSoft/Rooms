using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Tests;

public class RoomVerbTests
{

    [Theory]
    [InlineData("VERB")]
    public void TestParse(string verb) => Assert.Equal(verb, RoomVerb.Parse(verb).ToString());

}