
using System.Net;
using System.Net.Sockets;
using System.Text;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Sockets;

var mode = Console.ReadLine();
switch (mode)
{
    case "Server": await RunServer(); break;
    case "Client": await RunClient(); break;
}

async Task RunServer()
{
    using var listener = new TcpListener(IPAddress.Any, 55000);
    listener.Start();
    using var client = await listener.AcceptTcpClientAsync();
    listener.Stop();
    var stream = new RoomNetworkStream(client);
    //
    var protocol = new RoomProtocol();
    var data = new MemoryStream();
    await stream.ReadProtocolAsync(protocol);
    await stream.ReadContentAsync((long)protocol.Count, data);
    Console.WriteLine($"> {protocol.Verb}{protocol.Channel}{protocol.Count}[{data.Length} bytes]");
    data.Seek(0, SeekOrigin.Begin);
    await stream.WriteProtocolAsync(protocol);
    await stream.WriteContentAsync((long)protocol.Count, data);
}

async Task RunClient()
{
    var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, 55000);
    var stream = new RoomNetworkStream(client);
    var message = "MESSAGE CONTENT";
    //
    var data = new MemoryStream(Encoding.UTF8.GetBytes(message));
    var protocol = new RoomProtocol
    {
        Verb = RoomVerb.Parse("ECHO "),
        Channel = (RoomChannel)0,
        Count = (RoomCount)data.Length
    };
    await stream.WriteProtocolAsync(protocol);
    await stream.WriteContentAsync((long)protocol.Count, data);
    data.Seek(0, SeekOrigin.Begin);
    await stream.ReadProtocolAsync(protocol);
    await stream.ReadContentAsync((long)protocol.Count, data);
    Console.WriteLine($"> {protocol.Verb}{protocol.Channel}{protocol.Count}[{data.Length} bytes]");
    message = Encoding.UTF8.GetString(data.ToArray());
}