using System.Net.WebSockets;
using System.Text;

namespace KolibSoft.Rooms.Web;

public class RoomSocket : IDisposable
{

    private bool disposed = false;

    public byte[] SendBuffer { get; private set; }
    public byte[] ReceiveBuffer { get; private set; }

    public WebSocket Socket { get; }
    public bool IsAlive => Socket.State == WebSocketState.Open;

    public RoomSocket(WebSocket socket, int bufferingSize = 1024 * 1024)
    {
        SendBuffer = new byte[bufferingSize];
        ReceiveBuffer = new byte[bufferingSize];
        Socket = socket;
    }

    public async Task SendAsync(RoomMessage message)
    {
        var @string = message.ToString();
        if (Encoding.UTF8.GetByteCount(@string) > SendBuffer.Length / 2)
            throw new ApplicationException("Message is too large");
        var count = Encoding.UTF8.GetBytes(@string, SendBuffer);
        var segment = new ArraySegment<byte>(SendBuffer, 0, count);
        await Socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task<RoomMessage?> ReceiveAsync()
    {
        var result = await Socket.ReceiveAsync(ReceiveBuffer, CancellationToken.None);
        if (!result.EndOfMessage)
            throw new ApplicationException("Message is too large");
        if (result.MessageType == WebSocketMessageType.Text)
        {
            var segment = new ArraySegment<byte>(ReceiveBuffer, 0, result.Count);
            var @string = Encoding.UTF8.GetString(segment);
            var message = RoomMessage.Parse(@string);
            return message;
        }
        else if (result.MessageType == WebSocketMessageType.Close)
            await Socket.CloseOutputAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
        return null;

    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            SendBuffer = null!;
            ReceiveBuffer = null!;
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}