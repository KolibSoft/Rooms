using KolibSoft.Rooms.Core;

namespace KolibSoft.Rooms.Web;

public class Room(int code, int slots = 4, string? pass = null, string? tag = null)
{

    public int Code { get; } = code;
    public int Slots { get; } = slots;
    public string? Pass { get; } = pass;
    public string? Tag { get; } = tag;

    public RoomHub Hub { get; } = new();
    public int Count => Hub.Sockets.Count;
    public bool IsAlive { get; private set; } = false;

    public async Task JoinAsync(IRoomSocket socket, string? pass)
    {
        if (Count >= Slots || Pass != pass)
            throw new InvalidOperationException();
        await Hub.ListenAsync(socket);
    }

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