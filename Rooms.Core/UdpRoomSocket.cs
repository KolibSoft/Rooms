using System.Net.Sockets;

namespace KolibSoft.Rooms.Core;

public class UdpRoomSocket : IRoomSocket
{

    public UdpClient Client { get; }

    public bool IsAlive => true;

    public ArraySegment<byte> SendBuffer { get; }

    public async Task SendAsync(RoomMessage message)
    {
        if (message.Length > SendBuffer.Count)
            throw new IOException("Message is too big");
        message.CopyTo(SendBuffer);
        var data = SendBuffer.Slice(0, message.Length);
        await Client.SendAsync(data);
    }

    public async Task<RoomMessage> ReceiveAsync()
    {
        var result = await Client.ReceiveAsync();
        if (!RoomMessage.Verify(result.Buffer))
            throw new IOException("Invalid message received");
        var message = new RoomMessage(result.Buffer);
        return message;
    }

    public UdpRoomSocket(UdpClient client, ArraySegment<byte> sendBuffer)
    {
        Client = client;
        SendBuffer = sendBuffer;
    }

    public UdpRoomSocket(UdpClient client, int bufferingSize = 1024)
    {
        Client = client;
        SendBuffer = new byte[bufferingSize];
    }

}