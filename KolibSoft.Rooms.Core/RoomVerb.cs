using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KolibSoft.Rooms.Core;

public struct RoomVerb(ArraySegment<byte> data)
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

        var other = (RoomVerb)obj;
        return this == other;
    }

    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }

    public static RoomVerb Parse(string @string)
    {
        if (@string.Length != 3 || @string.Any(x => !char.IsUpper(x)))
            throw new FormatException();
        var data = Encoding.UTF8.GetBytes(@string);
        return new RoomVerb(data);
    }

    public static bool operator ==(RoomVerb lhs, RoomVerb rhs)
    {
        return lhs.Data.SequenceEqual(rhs.Data);
    }

    public static bool operator !=(RoomVerb lhs, RoomVerb rhs)
    {
        return !lhs.Data.SequenceEqual(rhs.Data);
    }

    public static readonly RoomVerb None = Parse("NOP");

}