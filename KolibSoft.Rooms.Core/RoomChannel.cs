using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KolibSoft.Rooms.Core;

public struct RoomChannel(ArraySegment<byte> utf8)
{

    public ArraySegment<byte> Data { get; } = utf8;

    public override string ToString()
    {
        return Encoding.UTF8.GetString(Data);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var other = (RoomChannel)obj;
        return this == other;
    }

    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }

    public static bool Verify(ReadOnlySpan<byte> utf8)
    {
        if (utf8.Length != 8) return false;
        for (var i = 0; i < utf8.Length; i++)
        {
            var c = utf8[i];
            if (c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f')
                continue;
            return false;
        }
        return true;
    }

    public static bool Verify(ReadOnlySpan<char> @string)
    {
        if (@string.Length != 8) return false;
        for (var i = 0; i < @string.Length; i++)
        {
            var c = @string[i];
            if (c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f')
                continue;
            return false;
        }
        return true;
    }

    public static RoomChannel Parse(ReadOnlySpan<byte> utf8)
    {
        if (!Verify(utf8))
            throw new FormatException($"Invalid channel format: {Encoding.UTF8.GetString(utf8)}");
        return new RoomChannel(utf8.ToArray());
    }

    public static RoomChannel Parse(ReadOnlySpan<char> @string)
    {
        if (!Verify(@string))
            throw new FormatException($"Invalid channel format: {@string}");
        var utf8 = new byte[8];
        Encoding.UTF8.GetBytes(@string, utf8);
        return new RoomChannel(utf8);
    }
    public static bool operator ==(RoomChannel lhs, RoomChannel rhs)
    {
        return lhs.Data.SequenceEqual(rhs.Data) || (int)lhs == (int)rhs;
    }

    public static bool operator !=(RoomChannel lhs, RoomChannel rhs)
    {
        return !lhs.Data.SequenceEqual(rhs.Data) && (int)lhs != (int)rhs;
    }

    public static implicit operator int(RoomChannel channel)
    {
        var @string = channel.ToString();
        var @int = Convert.ToInt32(@string, 16);
        return @int;
    }

    public static implicit operator RoomChannel(int @int)
    {
        var @string = @int.ToString();
        var channel = Parse(@string);
        return channel;
    }

    public static readonly RoomChannel Loopback = Parse("00000000");
    public static readonly RoomChannel Broadcast = Parse("ffffffff");

}