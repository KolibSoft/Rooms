using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KolibSoft.Rooms.Core;

public struct RoomVerb(ArraySegment<byte> utf8)
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

        var other = (RoomVerb)obj;
        return this == other;
    }

    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }

    public static bool Verify(ReadOnlySpan<byte> utf8)
    {
        if (utf8.Length != 3) return false;
        for (var i = 0; i < utf8.Length; i++)
        {
            var c = utf8[i];
            if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z')
                continue;
            return false;
        }
        return true;
    }

    public static bool Verify(ReadOnlySpan<char> @string)
    {
        if (@string.Length != 3) return false;
        for (var i = 0; i < @string.Length; i++)
        {
            var c = @string[i];
            if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z')
                continue;
            return false;
        }
        return true;
    }

    public static RoomVerb Parse(ReadOnlySpan<byte> utf8)
    {
        if (!Verify(utf8))
            throw new FormatException($"Invalid verb format: {Encoding.UTF8.GetString(utf8)}");
        return new RoomVerb(utf8.ToArray());
    }

    public static RoomVerb Parse(ReadOnlySpan<char> @string)
    {
        if (!Verify(@string))
            throw new FormatException($"Invalid verb format: {@string}");
        var utf8 = new byte[3];
        Encoding.UTF8.GetBytes(@string, utf8);
        return new RoomVerb(utf8);
    }

    public static bool operator ==(RoomVerb lhs, RoomVerb rhs)
    {
        return lhs.Data.SequenceEqual(rhs.Data);
    }

    public static bool operator !=(RoomVerb lhs, RoomVerb rhs)
    {
        return !lhs.Data.SequenceEqual(rhs.Data);
    }

    public static readonly RoomVerb None = Parse("NNN");

}