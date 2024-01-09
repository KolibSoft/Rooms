using System.Text;
using KolibSoft.Rooms.Core;

Console.WriteLine(RoomVerb.None);
Console.WriteLine(RoomChannel.Loopback);
Console.WriteLine(RoomContent.None);
Console.WriteLine(new RoomMessage());

var message = new RoomMessage()
{
    Verb = RoomVerb.Parse("MSG"),
    Channel = RoomChannel.Parse("1A1A1A1A"),
    Content = RoomContent.Parse("Message Text Content")
};

var @string = message.ToString();
Console.WriteLine(@string);
message = RoomMessage.Parse(@string);
Console.WriteLine(message);

message = new RoomMessage()
{
    Verb = RoomVerb.Parse("KCK"),
    Channel = RoomChannel.Parse("A1A1A1A1"),
    Content = RoomContent.Parse("Reason: Hacker")
};

