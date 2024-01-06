using System.Net.WebSockets;
using System.Text;

namespace KolibSoft.Rooms.Web;

public class RoomSocket
{

    public byte[] Buffer { get; }
    public WebSocket Socket { get; }
    public bool IsAlive => Socket.State == WebSocketState.Open;

    public RoomSocket(WebSocket socket, int bufferSize = 1024 * 1024)
    {
        Buffer = new byte[bufferSize];
        Socket = socket;
    }

    public async Task SendAsync(RoomMessage message)
    {
        var @string = message.ToString();
        if (Encoding.UTF8.GetByteCount(@string) > Buffer.Length)
            throw new ApplicationException("Message is too large");
        var count = Encoding.UTF8.GetBytes(@string, Buffer);
        await Socket.SendAsync(new ArraySegment<byte>(Buffer, 0, count), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task<RoomMessage?> ReceiveAsync()
    {
        var result = await Socket.ReceiveAsync(Buffer, CancellationToken.None);
        if (!result.EndOfMessage)
            throw new ApplicationException("Message is too large");
        if (result.MessageType == WebSocketMessageType.Text)
        {
            var @string = Encoding.UTF8.GetString(Buffer, 0, result.Count);
            var message = RoomMessage.Parse(@string);
            return message;
        }
        else if (result.MessageType == WebSocketMessageType.Close)
            await Socket.CloseOutputAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
        return null;

    }

}