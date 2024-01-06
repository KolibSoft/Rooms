using System.Net.WebSockets;
using System.Text;

namespace KolibSoft.Rooms.Web;

public class RoomSocket
{

    public int MaxMessageSize { get; }
    public WebSocket Socket { get; }
    public bool IsAlive => Socket.State == WebSocketState.Open;

    public RoomSocket(WebSocket socket, int maxMessageSize = 1024 * 1024)
    {
        Socket = socket;
        MaxMessageSize = maxMessageSize;
    }

    public async Task SendAsync(RoomMessage message)
    {
        var @string = message.ToString();
        if (Encoding.UTF8.GetByteCount(@string) > MaxMessageSize)
            throw new ApplicationException("Message is too large");
        var bytes = Encoding.UTF8.GetBytes(@string);
        await Socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task<RoomMessage?> ReceiveAsync()
    {
        var bytes = new byte[MaxMessageSize];
        var result = await Socket.ReceiveAsync(bytes, CancellationToken.None);
        if (!result.EndOfMessage)
            throw new ApplicationException("Message is too large");
        if (result.MessageType == WebSocketMessageType.Text)
        {
            var @string = Encoding.UTF8.GetString(bytes, 0, result.Count);
            var message = RoomMessage.Parse(@string);
            return message;
        }
        else if (result.MessageType == WebSocketMessageType.Close)
            await Socket.CloseOutputAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
        return null;

    }

}