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
        if (message.Length > SendBuffer.Count)
            throw new IOException("Message is too big");
        message.CopyTo(SendBuffer);
        var data = SendBuffer.Slice(0, message.Length);
        var stream = Client.GetStream();
        // TODO: Handle message fragmentation
        await stream.WriteAsync(data);
    }

    public async Task<RoomMessage> ReceiveAsync()
    {
        var stream = Client.GetStream();
        // TODO: Handle message fragmentation
        var count = await stream.ReadAsync(ReceiveBuffer);
        var data = ReceiveBuffer.Slice(0, count);
        var message = new RoomMessage(data);
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