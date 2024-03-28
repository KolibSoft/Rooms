using System.Diagnostics;
using KolibSoft.Rooms.Core;
using KolibSoft.Rooms.Core.Services;
using KolibSoft.Rooms.Core.Sockets;

namespace KolibSoft.Rooms.Web;

public class Room
{

    public int Code { get; }
    public int Slots { get; }
    public string? Pass { get; }
    public string? Tag { get; }

    public RoomHub Hub { get; } = new();
    public int Count => Hub.Sockets.Length;
    public bool IsAlive { get; private set; } = false;

    public async Task JoinAsync(IRoomSocket socket, string? pass)
    {
        if (Count >= Slots || Pass != pass)
            throw new InvalidOperationException();
        await Hub.ListenAsync(socket, 1024 * 1024);
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

    public Room(int code, int slots = 4, string? pass = null, string? tag = null)
    {
        Code = code;
        Slots = slots;
        Pass = pass;
        Tag = tag;
        Hub.Logger = Console.Out;
    }

}