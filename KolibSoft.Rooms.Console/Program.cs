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

message = "KCK 87654321\nHacker".ToMessage();
Console.WriteLine(message);
