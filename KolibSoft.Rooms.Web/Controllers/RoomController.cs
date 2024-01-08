using KolibSoft.Rooms.Core;
using Microsoft.AspNetCore.Mvc;

namespace KolibSoft.Rooms.Web;

[Route("/api/rooms")]
public class RoomController : ControllerBase
{

    public static List<(int code, int slots, string? pass, string? tag, RoomHub hub)> Rooms { get; } = new();

    [HttpGet]
    public async Task ConnectAsync(int? code = null, string? pass = null)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var room = Rooms.Where(x => x.code == code && x.hub.Sockets.Length < x.slots && x.pass == pass);
            if (room != null)
            {
                var wsocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                
            }
        }
    }

}