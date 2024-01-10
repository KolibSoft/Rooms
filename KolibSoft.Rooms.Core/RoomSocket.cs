using System.Net.WebSockets;

namespace KolibSoft.Rooms.Core;

public class RoomSocket(WebSocket socket, int bufferingSize = 1024)
{

    public WebSocket Socket { get; } = socket;
    public bool IsAlive => Socket.State == WebSocketState.Open;
    public ArraySegment<byte> SendBuffer { get; } = new byte[bufferingSize];
    public ArraySegment<byte> ReceiveBuffer { get; } = new byte[bufferingSize];

    public async Task SendAsync(RoomMessage message)
    {
        message.CopyTo(SendBuffer);
        var data = SendBuffer.Slice(0, message.Length);
        await Socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task<RoomMessage?> ReceiveAsync()
    {
        var result = await Socket.ReceiveAsync(ReceiveBuffer, CancellationToken.None);
        var data = ReceiveBuffer.Slice(0, result.Count);
        if (result.MessageType != WebSocketMessageType.Text || !result.EndOfMessage || !RoomMessage.Verify(data))
        {
            await Socket.CloseOutputAsync(result.CloseStatus ?? WebSocketCloseStatus.ProtocolError, result.CloseStatusDescription, CancellationToken.None);
            return null;
        }
        var message = new RoomMessage(data.ToArray());
        return message;
    }

    public const string Protocol = "Room";

}