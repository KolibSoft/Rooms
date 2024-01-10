namespace KolibSoft.Rooms.Core;

/// <summary>
/// A standard room instance representation.
/// </summary>
/// <param name="code">Identification code.</param>
/// <param name="slots">Max participants count.</param>
/// <param name="pass">Pass code.</param>
/// <param name="tag">Metadata to search.</param>
public class Room(int code, int slots = 4, string? pass = null, string? tag = null)
{

    /// <summary>
    /// Identification code.
    /// </summary>
    public int Code { get; } = code;

    /// <summary>
    /// Max participants count.
    /// </summary>
    public int Slots { get; } = slots;

    /// <summary>
    /// Pass code.
    /// </summary>
    public string? Pass { get; } = pass;

    /// <summary>
    /// Metadata to search.
    /// </summary>
    public string? Tag { get; } = tag;

    /// <summary>
    /// The underlying hub.
    /// </summary>
    public RoomHub Hub { get; } = new();

    /// <summary>
    /// Participant count.
    /// </summary>
    public int Count => Hub.Sockets.Length;

    /// <summary>
    /// Check if is running.
    /// </summary>
    public bool IsAlive { get; private set; } = false;

    /// <summary>
    /// Attempts to join the socket and start listening it.
    /// </summary>
    /// <param name="socket">Connected socket.</param>
    /// <param name="pass">Pass code.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task JoinAsync(RoomSocket socket, string? pass)
    {
        if (Count >= Slots || Pass != pass)
            throw new InvalidOperationException();
        await Hub.ListenAsync(socket);
    }

    /// <summary>
    /// Starts to route and send the received messages.
    /// </summary>
    /// <param name="ttl">Time to wait for connections until stop.</param>
    public async void RunAsync(TimeSpan ttl)
    {
        if (!IsAlive)
        {
            IsAlive = true;
            DateTime tp = default;
            tp = DateTime.UtcNow + ttl;
            while (Count == 0 && DateTime.UtcNow < tp)
                await Task.Delay(100);
            while (Count > 0)
            {
                await Hub.TransmitAsync();
                tp = DateTime.UtcNow + ttl;
                while (Count == 0 && DateTime.UtcNow < tp)
                    await Task.Delay(100);
            }
            IsAlive = false;
        }
    }

}