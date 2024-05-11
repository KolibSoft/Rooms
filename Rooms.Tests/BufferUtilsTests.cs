using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Socekts;

namespace KolibSoft.Rooms.Tests;

public class BufferUtilsTests
{

    [Fact]
    public void TestUtils()
    {
        var buffer = new byte[1024];
        var verb = RoomVerb.Parse("VERB");
        var channel = RoomChannel.Parse("0000");
        var count = RoomCount.Parse("21");
        buffer.AsSpan()
            .WriteVerb(verb)
            .WriteValue((byte)' ')
            .WriteChannel(channel)
            .WriteValue((byte)' ')
            .WriteCount(count)
            .WriteValue((byte)'\n')
            ;
        verb = default;
        channel = default;
        count = default;
        byte value;
        ((ReadOnlySpan<byte>)buffer.AsSpan())
            .ReadVerb(out verb)
            .ReadValue(out value)
            .ReadChannel(out channel)
            .ReadValue(out value)
            .ReadCount(out count)
            .ReadValue(out value)
        ;
    }

}