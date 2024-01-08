using System.Text;
using KolibSoft.Rooms.Core;

Console.WriteLine(RoomVerb.None);
Console.WriteLine(RoomChannel.None);
Console.WriteLine(RoomContent.None);
Console.WriteLine(new RoomMessage());

var message = new RoomMessage()
{
    Verb = RoomVerb.Parse("MSG"),
    Channel = RoomChannel.Parse("12345678"),
    Content = RoomContent.Parse("Message Text Content")
};

var @string = message.ToString();
Console.WriteLine(@string);
message = RoomMessage.Parse(@string);
Console.WriteLine(message);

message = new RoomMessage()
{
    Verb = RoomVerb.Parse("KCK"),
    Channel = RoomChannel.Parse("87654321"),
    Content = RoomContent.Parse("Reason: Hacker")
};
var buffer = new RoomBuffer();
buffer.SetMessage(message);
Console.WriteLine(buffer.GetMessage());

