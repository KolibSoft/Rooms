using KolibSoft.Rooms.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace KolibSoft.Rooms.Web;

[EnableCors("PUBLIC")]
[Route("/api/rooms")]
public class RoomController : ControllerBase
{

    public static List<Room> Rooms { get; private set; } = new();

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? hint = null)
    {
        var rooms = Rooms.Where(x => x.IsAlive);
        if (hint != null) rooms = rooms.Where(x => x.Tag?.Contains(hint, StringComparison.InvariantCultureIgnoreCase) == true);
        var items = rooms.Select(x => new RoomItem
        {
            Code = x.Code,
            Count = x.Count,
            Slots = x.Slots,
            Pass = x.Pass != null,
            Tag = x.Tag
        }).ToArray();
        return Ok(items);
    }

    [HttpGet("join")]
    public async Task JoinAsync([FromQuery] int code, [FromQuery] int slots = 4, [FromQuery] string? pass = null, [FromQuery] string? tag = null)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var room = Rooms.FirstOrDefault(x => x.Code == code);
            if (room == null)
            {
                var index = Rooms.FindIndex(x => !x.IsAlive);
                if (index < 0 && Rooms.Count >= 128) return;
                room = new Room(code, int.Min(slots, 16), pass?.Substring(0, 32), tag?.Substring(0, 128));
                if (index >= 0) Rooms[index] = room;
                else Rooms.Add(room);
            }
            room.RunAsync(TimeSpan.FromSeconds(16));
            if (room.Count < room.Slots && room.Pass == pass)
            {
                var wsocket = await HttpContext.WebSockets.AcceptWebSocketAsync(RoomSocket.Protocol);
                var rsocket = new RoomSocket(wsocket, 1024);
                await room.JoinAsync(rsocket, pass);
            }
        }
    }

}