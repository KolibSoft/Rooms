namespace KolibSoft.Rooms.Core;

public interface IRoomSocket
{
    public bool IsAlive { get; }
    public Task SendAsync(RoomMessage message);
    public Task<RoomMessage> ReceiveAsync();
}