using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KolibSoft.Rooms.Core;

public struct RoomChannel(ArraySegment<byte> data)
{

    public ArraySegment<byte> Data { get; } = data;

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

    public static RoomChannel Parse(string @string)
    {
        if (@string.Length != 8 || !@string.All(char.IsAsciiHexDigit))
            throw new FormatException($"Invalid channel format: {@string}");
        var data = Encoding.UTF8.GetBytes(@string);
        return new RoomChannel(data);
    }

    public static bool operator ==(RoomChannel lhs, RoomChannel rhs)
    {
        return (int)lhs == (int)rhs;
    }

    public static bool operator !=(RoomChannel lhs, RoomChannel rhs)
    {
        return (int)lhs != (int)rhs;
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