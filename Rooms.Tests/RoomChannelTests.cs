using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Tests;

public class RoomChannelTests
{

    [Theory]
    [InlineData("FF")]
    public void TestParse(string channel) => Assert.Equal(channel, RoomChannel.Parse(channel).ToString());

    [Theory]
    [InlineData(255)]
    public void TestCast(int channel) => Assert.Equal(channel, (int)(RoomChannel)channel);


}