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
        var content = RoomContent.Create("V2VsY29tZSB0byBCYXNlNjQgc3RyaW5nIGlzIGdvb2Qh");
        var count = (RoomCount)content.Data.Length;
        buffer.AsSpan()
            .WriteVerb(verb)
            .WriteValue((byte)' ')
            .WriteChannel(channel)
            .WriteValue((byte)' ')
            .WriteCount(count)
            .WriteValue((byte)'\n')
            .WriteContent(content)
            ;
        verb = default;
        channel = default;
        content = default;
        count = default;
        byte value;
        ((ReadOnlySpan<byte>)buffer.AsSpan())
            .ReadVerb(out verb)
            .ReadValue(out value)
            .ReadChannel(out channel)
            .ReadValue(out value)
            .ReadCount(out count)
            .ReadValue(out value)
            .ReadContent(out content)
        ;
    }

}