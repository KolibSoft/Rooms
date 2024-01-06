using KolibSoft.Rooms.Web;

var message = new Message();
message.Headers["ID"] = "ID".GetHashCode().ToString();
message.Headers["Custom-Header"] = "Custom-Value";
message.Body = "Message Body\n";

var @string = message.ToString();
message = Message.Parse(@string);
Console.WriteLine(@string);
Console.WriteLine(message);

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
