using System.Net.WebSockets;

namespace KolibSoft.Rooms.Core;

/// <summary>
/// A buffering web socket to send and receive messages asynchronously.
/// </summary>
public class RoomSocket
{

    /// <summary>
    /// The underlying provided Web Socket.
    /// </summary>
    public WebSocket Socket { get; }

    /// <summary>
    /// Checks if the underlying Web Socket is open.
    /// </summary>
    public bool IsAlive => Socket.State == WebSocketState.Open;

    /// <summary>
    /// The underlying Send Buffer.
    /// </summary>
    public ArraySegment<byte> SendBuffer { get; }

    /// <summary>
    /// The underlying Receive Buffer.
    /// </summary>
    public ArraySegment<byte> ReceiveBuffer { get; }

    /// <summary>
    /// Send a message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    public async Task SendAsync(RoomMessage message)
    {
        if (message.Length > SendBuffer.Count)
        {
            await Socket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError, null, CancellationToken.None);
            throw new IOException("Message is too big");
        }
        message.CopyTo(SendBuffer);
        var data = SendBuffer.Slice(0, message.Length);
        await Socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    /// <summary>
    /// Receive a message asynchronously. Close the underlying Web Socket if an invalid message is received.
    /// </summary>
    /// <returns>The message received.</returns>
    /// <exception cref="IOException"></exception>
    public async Task<RoomMessage> ReceiveAsync()
    {
        var result = await Socket.ReceiveAsync(ReceiveBuffer, CancellationToken.None);
        var data = ReceiveBuffer.Slice(0, result.Count);
        if (result.MessageType == WebSocketMessageType.Close)
        {
            await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            return new RoomMessage();
        }
        if (result.MessageType == WebSocketMessageType.Binary)
        {
            await Socket.CloseOutputAsync(WebSocketCloseStatus.InvalidMessageType, "Only text messages are allowed", CancellationToken.None);
            throw new IOException("Invalid message type received");
        }
        if (!result.EndOfMessage)
        {
            await Socket.CloseOutputAsync(WebSocketCloseStatus.MessageTooBig, $"Max allowed message size: {ReceiveBuffer.Count}", CancellationToken.None);
            throw new IOException("Too big message received");
        }
        if (!RoomMessage.Verify(data))
        {
            await Socket.CloseOutputAsync(WebSocketCloseStatus.ProtocolError, "Invalid message format", CancellationToken.None);
            throw new IOException("Invalid message received");
        }
        var message = new RoomMessage(data.ToArray());
        return message;
    }

    /// <summary>
    /// Creates a new Socket with the specified buffers.
    /// </summary>
    /// <param name="socket">A connected Web Socket.</param>
    /// <param name="sendBuffer">Send buffer.</param>
    /// <param name="receiveBuffer">Receive buffer.</param>
    public RoomSocket(WebSocket socket, ArraySegment<byte> sendBuffer, ArraySegment<byte> receiveBuffer)
    {
        Socket = socket;
        SendBuffer = sendBuffer;
        ReceiveBuffer = receiveBuffer;
    }

    /// <summary>
    /// Creates a new Socket with the specified buffering size.
    /// </summary>
    /// <param name="socket">A connected Web Socket.</param>
    /// <param name="bufferingSize">Buffering size to send and receive messages.</param>
    public RoomSocket(WebSocket socket, int bufferingSize = 1024)
    {
        Socket = socket;
        SendBuffer = new byte[bufferingSize];
        ReceiveBuffer = new byte[bufferingSize];
    }

    /// <summary>
    /// Room protocol name.
    /// </summary>
    public const string Protocol = "Room";

}