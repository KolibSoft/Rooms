using System.Text;

namespace KolibSoft.Rooms.Core;

/// <summary>
/// Represents a hub message.
/// </summary>
public class RoomMessage
{

    /// <summary>
    /// The message verb component.
    /// </summary>
    public RoomVerb Verb { get; set; }

    /// <summary>
    /// The message channel component.
    /// </summary>
    public RoomChannel Channel { get; set; }

    /// <summary>
    /// The message content component.
    /// </summary>
    public RoomContent Content { get; set; }

    /// <summary>
    /// The message total size in bytes (format blanks included).
    /// </summary>
    public int Length => Verb.Data.Count + Channel.Data.Count + Content.Data.Count + 2;

    /// <summary>
    /// Copies the message into a data buffer.
    /// </summary>
    /// <param name="data">Data buffer.</param>
    public void CopyTo(ArraySegment<byte> data)
    {
        Verb.Data.CopyTo(data.Slice(0, 3));
        Channel.Data.CopyTo(data.Slice(4, 8));
        Content.Data.CopyTo(data.Slice(13));
        data[3] = (byte)' ';
        data[12] = (byte)'\n';
    }

    /// <summary>
    /// Gets the string representation of the message.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Verb} {Channel}\n{Content}";
    }

    /// <summary>
    /// Constructs a message with the UTF8 text provided whitout validate it.
    /// </summary>
    /// <param name="data">UTF8 text</param>
    public RoomMessage(ArraySegment<byte> data)
    {
        Verb = new RoomVerb(data.Slice(0, 3));
        Channel = new RoomChannel(data.Slice(4, 8));
        Content = new RoomContent(data.Slice(13));
    }

    /// <summary>
    /// Constructs a default empty message
    /// </summary>
    public RoomMessage()
    {
        Verb = RoomVerb.None;
        Channel = RoomChannel.Loopback;
        Content = RoomContent.None;
    }

    /// <summary>
    /// Verify if the provided UTF8 text is a valid message.
    /// </summary>
    /// <param name="utf8">UTF8 text</param>
    /// <returns></returns>
    public static bool Verify(ReadOnlySpan<byte> utf8)
    {
        if (utf8.Length < 13) return false;
        var result = RoomVerb.Verify(utf8.Slice(0, 3)) && RoomChannel.Verify(utf8.Slice(4, 8)) && utf8[3] == ' ' && utf8[12] == '\n';
        return result;
    }

    /// <summary>
    /// Verify if the provided string is a valid message.
    /// </summary>
    /// <param name="string">String</param>
    /// <returns></returns>
    public static bool Verify(ReadOnlySpan<char> @string)
    {
        if (@string.Length < 13) return false;
        var result = RoomVerb.Verify(@string.Slice(0, 3)) && RoomChannel.Verify(@string.Slice(4, 8)) && @string[3] == ' ' && @string[12] == '\n';
        return result;
    }

    /// <summary>
    /// Parses an UTF8 text into a message.
    /// </summary>
    /// <param name="utf8">UTF8 text.</param>
    /// <returns></returns>
    public static RoomMessage Parse(ReadOnlySpan<byte> utf8)
    {
        if (!Verify(utf8))
            throw new FormatException($"Invalid message format: {Encoding.UTF8.GetString(utf8)}");
        var message = new RoomMessage(utf8.ToArray());
        return message;
    }

    /// <summary>
    /// Parses an string into a message.
    /// </summary>
    /// <param name="string">String</param>
    /// <returns></returns>
    public static RoomMessage Parse(ReadOnlySpan<char> @string)
    {
        if (!Verify(@string))
            throw new FormatException($"Invalid message format: {@string}");
        var utf8 = new byte[Encoding.UTF8.GetByteCount(@string)];
        Encoding.UTF8.GetBytes(@string, utf8);
        var message = new RoomMessage(utf8);
        return message;
    }

}