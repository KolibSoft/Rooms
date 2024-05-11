using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Tests;

public class RoomCountTests
{

    [Theory]
    [InlineData("255")]
    public void TestParse(string count) => Assert.Equal(count, RoomCount.Parse(count).ToString());

    [Theory]
    [InlineData(255)]
    public void TestCast(int count) => Assert.Equal(count, (int)(RoomCount)count);


}