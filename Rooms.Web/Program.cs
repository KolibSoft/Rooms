var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("PUBLIC", options => options.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());
});
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors();
app.UseWebSockets();
app.UseFileServer();
app.MapControllers();

app.Run();
