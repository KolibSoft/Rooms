using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KolibSoft.Rooms.Core;

public struct RoomContent(ArraySegment<byte> utf8)
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

        var other = (RoomContent)obj;
        return this == other;
    }

    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }

    public static RoomContent Parse(ReadOnlySpan<byte> utf8)
    {
        return new RoomContent(utf8.ToArray());
    }

    public static RoomContent Parse(ReadOnlySpan<char> @string)
    {
        var utf8 = new byte[Encoding.UTF8.GetByteCount(@string)];
        Encoding.UTF8.GetBytes(@string, utf8);
        return new RoomContent(utf8);
    }

    public static bool operator ==(RoomContent lhs, RoomContent rhs)
    {
        return lhs.Data.SequenceEqual(rhs.Data);
    }

    public static bool operator !=(RoomContent lhs, RoomContent rhs)
    {
        return !lhs.Data.SequenceEqual(rhs.Data);
    }

    public static readonly RoomContent None = Parse("");

}