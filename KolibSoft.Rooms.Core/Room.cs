namespace KolibSoft.Rooms.Core;

public class Room(int code, int slots = 4, string? pass = null, string? tag = null)
{

    public int Code { get; } = code;
    public int Slots { get; } = slots;
    public string? Pass { get; } = pass;
    public string? Tag { get; } = tag;

    public RoomHub Hub { get; } = new();

    public async Task JoinAsync(RoomSocket socket, string? pass)
    {
        if (Hub.Sockets.Length >= Slots || Pass != pass)
            throw new InvalidOperationException();
        await Hub.ListenAsync(socket);
    }

    public async Task RunAsync()
    {
        while (true)
        {
            await Hub.TransmitAsync();
            await Task.Delay(100);
        }
    }

}