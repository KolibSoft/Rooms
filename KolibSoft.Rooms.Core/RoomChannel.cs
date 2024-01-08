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
        if (@string.Length != 8 || @string.Any(x => !char.IsDigit(x)))
            throw new FormatException();
        var data = Encoding.UTF8.GetBytes(@string);
        return new RoomChannel(data);
    }

    public static bool operator ==(RoomChannel lhs, RoomChannel rhs)
    {
        return lhs.Data.SequenceEqual(rhs.Data);
    }

    public static bool operator !=(RoomChannel lhs, RoomChannel rhs)
    {
        return !lhs.Data.SequenceEqual(rhs.Data);
    }

    public static readonly RoomChannel None = RoomChannel.Parse("00000000");

}