using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KolibSoft.Rooms.Core;

/// <summary>
/// Represents a variable length UTF8 text.
/// </summary>
/// <param name="utf8">UTF8 text.</param>
public struct RoomContent(ArraySegment<byte> utf8)
{

    /// <summary>
    /// UTF8 internal data.
    /// </summary>
    public ArraySegment<byte> Data { get; } = utf8;

    public int Length => Data.Count;

    /// <summary>
    /// Gets the string representation of the content.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Parses an UTF8 text into a content.
    /// </summary>
    /// <param name="utf8">UTF8 text.</param>
    /// <returns></returns>
    public static RoomContent Parse(ReadOnlySpan<byte> utf8)
    {
        return new RoomContent(utf8.ToArray());
    }

    /// <summary>
    /// Parses an string into a content.
    /// </summary>
    /// <param name="string">String.</param>
    /// <returns></returns>
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

    /// <summary>
    /// A default empty content.
    /// </summary>
    public static readonly RoomContent None = Parse("");

}