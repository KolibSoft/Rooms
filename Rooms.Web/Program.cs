using System.Collections.Immutable;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Services;
using KolibSoft.Rooms.Core.Streams;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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

[EnableCors("PUBLIC")]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{

    public async Task<IActionResult> Index([FromQuery] string? hint = null)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            using var stream = new RoomWebStream(socket);
            var message = await stream.ReadMessageAsync();
            if (message.Verb != "OPTIONS")
            {
                await stream.WriteMessageAsync(new RoomMessage
                {
                    Verb = "INFO",
                    Content = await RoomContentUtils.CreateAsTextAsync("Room Info Required")
                });
                return BadRequest();
            }
            var info = await message.Content.ReadAsJsonAsync<RoomInfo>() ?? new RoomInfo();
            var room = Rooms.FirstOrDefault(x => x.Info.Name == info.Name);
            if (room == null)
            {
                if (string.IsNullOrWhiteSpace(info.Name)) info.Name = info.GetHashCode().ToString();
                info.Slots = int.Max(2, int.Min(info.Slots, 16));
                room = new Room(info);
                room.Hub.Start();
                Rooms = Rooms.Add(room);
            }
            else if (room.Info.Password != info.Password)
            {
                await stream.WriteMessageAsync(new RoomMessage
                {
                    Verb = "INFO",
                    Content = await RoomContentUtils.CreateAsTextAsync("Wrong room password")
                });
                return BadRequest();
            }
            else if (room.Count >= room.Info.Slots)
            {
                await stream.WriteMessageAsync(new RoomMessage
                {
                    Verb = "INFO",
                    Content = await RoomContentUtils.CreateAsTextAsync("Room is full")
                });
                return BadRequest();
            }
            await stream.WriteMessageAsync(new RoomMessage
            {
                Verb = "OPTIONS",
                Content = await RoomContentUtils.CreateAsJsonAsync(new
                {
                    Name = info.Name,
                    Tag = info.Tag,
                    HasPassword = info.Password != null,
                    Slots = info.Slots,
                    Count = room.Count
                })
            });
            await room.Hub.ListenAsync(stream);
            return Ok();
        }
        else
        {
            var dump = Rooms.Where(x => x.Count == 0);
            foreach (var room in dump) room.Hub.Stop();
            Rooms = Rooms.RemoveRange(dump);
            if (string.IsNullOrWhiteSpace(hint))
            {
                return Ok(Rooms.Select(x => new
                {
                    Name = x.Info.Name,
                    Tag = x.Info.Tag,
                    HasPassword = x.Info.Password != null,
                    Slots = x.Info.Slots,
                    Count = x.Count
                }).OrderBy(x => x.Name));
            }
            else
            {
                hint = hint.ToUpper();
                return Ok(Rooms.Where(x => x.Info.Name.ToUpper().Contains(hint)).Select(x => new
                {
                    Name = x.Info.Name,
                    Tag = x.Info.Tag,
                    HasPassword = x.Info.Password != null,
                    Slots = x.Info.Slots,
                    Count = x.Count
                }).OrderBy(x => x.Name));
            }
        }
    }

    public static ImmutableList<Room> Rooms { get; private set; } = [];

}

public class Room
{

    public RoomInfo Info { get; private set; }
    public int Count => Hub.Count;

    public RoomHub Hub { get; private set; } = new RoomHub();

    public Room(RoomInfo info)
    {
        Info = info;
    }

}

public class RoomInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public string? Password { get; set; }
    public int Slots { get; set; } = 4;
}