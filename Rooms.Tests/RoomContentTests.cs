using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Tests;

public class RoomContentTests
{

    [Theory]
    [InlineData("V2VsY29tZSB0byBCYXNlNjQgc3RyaW5nIGlzIGdvb2Qh")]
    public void TestCreate(string content) => Assert.Equal(content, RoomContent.Create(content).ToString());

}