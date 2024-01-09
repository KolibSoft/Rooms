using KolibSoft.Rooms.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace KolibSoft.Rooms.Web;

[Route("/api/rooms")]
public class RoomController : ControllerBase
{

    public static List<Room> Rooms { get; } = new();

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? tag = null)
    {
        var rooms = Rooms.Where(x => x.IsAlive);
        if (tag != null) rooms = rooms.Where(x => x.Tag == tag);
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
                room = new Room(code, slots, pass, tag);
                Rooms.Add(room);
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