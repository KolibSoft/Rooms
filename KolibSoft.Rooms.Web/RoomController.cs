using KolibSoft.Rooms.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace KolibSoft.Rooms.Web;

[EnableCors("PUBLIC")]
[Route("/api/rooms")]
public class RoomController : ControllerBase
{

    public static List<Room> Rooms { get; private set; } = new();
    public static int Buffering = 512 * 1024 * 1024;
    public static int BufferingUsage => Rooms.Sum(x => x.Hub.Sockets.Sum(x => x.SendBuffer.Count + x.ReceiveBuffer.Count));

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
    public async Task JoinAsync([FromQuery] int code, [FromQuery] int slots = 4, [FromQuery] string? pass = null, [FromQuery] string? tag = null, [FromQuery] int buffering = 1024)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            code = int.Parse(code.ToString().PadLeft(8, '0')[^8..]);
            slots = int.Max(2, int.Min(slots, 16));
            pass = pass?[32..];
            tag = tag?[128..];
            buffering = int.Max(1024, int.Min(buffering, 16 * 1024 * 1024));
            if (BufferingUsage + buffering * 2 > Buffering) return;
            //
            var room = Rooms.FirstOrDefault(x => x.Code == code);
            if (room == null)
            {
                var index = Rooms.FindIndex(x => !x.IsAlive);
                room = new Room(code, slots, pass, tag);
                if (index >= 0) Rooms[index] = room;
                else Rooms.Add(room);
            }
            //
            room.RunAsync(TimeSpan.FromSeconds(16));
            if (room.Count < room.Slots && room.Pass == pass)
            {
                var wsocket = await HttpContext.WebSockets.AcceptWebSocketAsync(RoomSocket.Protocol);
                var rsocket = new RoomSocket(wsocket, buffering);
                await room.JoinAsync(rsocket, pass);
            }
        }
    }

}