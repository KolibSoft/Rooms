using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Tests;

public class RoomChannelTests
{

    [Theory]
    [InlineData("FF")]
    public void TestParse(string channel) => Assert.Equal(channel, RoomChannel.Parse(channel).ToString());

    [Theory]
    [InlineData(255)]
    public void TestCast(long channel) => Assert.Equal(channel, (long)(RoomChannel)channel);


}