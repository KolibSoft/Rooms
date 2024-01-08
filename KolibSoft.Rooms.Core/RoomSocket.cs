using System.Net.WebSockets;

namespace KolibSoft.Rooms.Core;

public class RoomSocket(WebSocket socket)
{

    public WebSocket Socket { get; } = socket;
    public bool IsAlive => Socket.State == WebSocketState.Open;

}