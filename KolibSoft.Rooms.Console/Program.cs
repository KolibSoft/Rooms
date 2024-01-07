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