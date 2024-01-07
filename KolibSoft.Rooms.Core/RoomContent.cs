using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KolibSoft.Rooms.Core;

public struct RoomContent(ArraySegment<byte> data)
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

        var other = (RoomContent)obj;
        return this == other;
    }

    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }

    public static RoomContent Parse(string @string)
    {
        var data = Encoding.UTF8.GetBytes(@string);
        return new RoomContent(data);
    }

    public static bool operator ==(RoomContent lhs, RoomContent rhs)
    {
        return lhs.Data.SequenceEqual(rhs.Data);
    }

    public static bool operator !=(RoomContent lhs, RoomContent rhs)
    {
        return !lhs.Data.SequenceEqual(rhs.Data);
    }

    public static readonly RoomContent None = RoomContent.Parse("");

}