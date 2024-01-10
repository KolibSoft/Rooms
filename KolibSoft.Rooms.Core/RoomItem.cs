namespace KolibSoft.Rooms.Core;

/// <summary>
/// A standard room info representation.
/// </summary>
public class RoomItem
{

    /// <summary>
    /// Identifier code of the room.
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Participants in the room.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Max participants count.
    /// </summary>
    public int Slots { get; set; }

    /// <summary>
    /// Pass phrase to join.
    /// </summary>
    public bool Pass { get; set; }

    /// <summary>
    /// Metadata to search.
    /// </summary>
    public string? Tag { get; set; }

}