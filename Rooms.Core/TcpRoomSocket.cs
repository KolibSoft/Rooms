using System.Net.Sockets;

namespace KolibSoft.Rooms.Core;

public class TcpRoomSocket : IRoomSocket
{

    public TcpClient Client { get; }

    public bool IsAlive => Client.Connected;

    public ArraySegment<byte> SendBuffer { get; }
    public ArraySegment<byte> ReceiveBuffer { get; }

    public async Task SendAsync(RoomMessage message)
    {
        if (!message.Validate())
        {
            Client.Close();
            throw new FormatException($"Invalid message format: {message}");
        }
        if (message.Length > SendBuffer.Count)
        {
            Client.Close();
            throw new IOException("Message is too big");
        }
        message.CopyTo(SendBuffer);
        var data = SendBuffer.Slice(0, message.Length);
        var stream = Client.GetStream();
        await stream.WriteAsync(data);
    }

    public async Task<RoomMessage> ReceiveAsync()
    {
        var stream = Client.GetStream();
        var count = await stream.ReadAsync(ReceiveBuffer);
        if (stream.DataAvailable)
        {
            Client.Close();
            throw new IOException("Message is too big");
        }
        var data = ReceiveBuffer.Slice(0, count);
        var message = new RoomMessage(data);
        if (!message.Validate())
        {
            Client.Close();
            throw new FormatException($"Invalid message format: {message}");
        }
        return message;
    }

    public TcpRoomSocket(TcpClient client, ArraySegment<byte> sendBuffer, ArraySegment<byte> receiveBuffer)
    {
        Client = client;
        SendBuffer = sendBuffer;
        ReceiveBuffer = receiveBuffer;
    }

    public TcpRoomSocket(TcpClient client, int bufferingSize = 1024)
    {
        Client = client;
        SendBuffer = new byte[bufferingSize];
        ReceiveBuffer = new byte[bufferingSize];
    }

}