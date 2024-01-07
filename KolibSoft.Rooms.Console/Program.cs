using System.Text;
using KolibSoft.Rooms.Core;

Console.WriteLine(RoomVerb.None);
Console.WriteLine(RoomIdentifier.None);
Console.WriteLine(RoomContent.None);
Console.WriteLine(new RoomMessage());

var message = new RoomMessage()
{
    Verb = RoomVerb.Parse("MSG"),
    Identifier = RoomIdentifier.Parse("12345678"),
    Content = RoomContent.Parse("Message Text Content")
};

var @string = message.ToString();
Console.WriteLine(@string);
message = RoomMessage.Parse(@string);
Console.WriteLine(message);

var rawMessage = Encoding.UTF8.GetBytes("KCK 87654321\nHacker");
message = new RoomMessage()
{
    Verb = new RoomVerb(new ArraySegment<byte>(rawMessage, 0, 3)),
    Identifier = new RoomIdentifier(new ArraySegment<byte>(rawMessage, 4, 8)),
    Content = new RoomContent(new ArraySegment<byte>(rawMessage, 13, rawMessage.Length - 13))
};
Console.WriteLine(message);
