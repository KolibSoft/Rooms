var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();
app.UseWebSockets();
app.UseFileServer();
app.MapControllers();

app.Run();
