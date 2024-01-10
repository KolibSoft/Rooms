using System.Net.WebSockets;

namespace KolibSoft.Rooms.Core;

/// <summary>
/// A buffering web socket to send and receive messages asynchronously.
/// </summary>
/// <param name="socket">A connected Web Socket.</param>
/// <param name="bufferingSize">Buffering size to send and receive messages.</param>
public class RoomSocket(WebSocket socket, int bufferingSize = 1024)
{

    /// <summary>
    /// The underlying provided Web Socket.
    /// </summary>
    public WebSocket Socket { get; } = socket;

    /// <summary>
    /// Checks if the underlying Web Socket is open.
    /// </summary>
    public bool IsAlive => Socket.State == WebSocketState.Open;

    /// <summary>
    /// The underlying Send Buffer.
    /// </summary>
    public ArraySegment<byte> SendBuffer { get; } = new byte[bufferingSize];

    /// <summary>
    /// The underlying Receive Buffer.
    /// </summary>
    public ArraySegment<byte> ReceiveBuffer { get; } = new byte[bufferingSize];

    /// <summary>
    /// Send a message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns></returns>
    public async Task SendAsync(RoomMessage message)
    {
        message.CopyTo(SendBuffer);
        var data = SendBuffer.Slice(0, message.Length);
        await Socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    /// <summary>
    /// Receive a message asynchronously. Close the underlying Web Socket if an invalid message is received.
    /// </summary>
    /// <returns>The message received.</returns>
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

    /// <summary>
    /// Room protocol name.
    /// </summary>
    public const string Protocol = "Room";

}