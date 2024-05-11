using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Tests;

public class BufferUtilsTests
{

    [Fact]
    public void TestUtils()
    {
        var verb = RoomVerb.Parse("VERB");
        var channel = RoomChannel.Parse("255");
        var count = RoomCount.Parse("21");
        var buffer = new byte[1024];
        BufferUtils.WriteRoomHead(buffer, ref verb, ref channel, ref count);
        verb = default;
        channel = default;
        count = default;
        BufferUtils.ReadRoomHead(buffer, ref verb, ref channel, ref count);
    }


}