using KolibSoft.Rooms.Core;
using Microsoft.AspNetCore.Mvc;

namespace KolibSoft.Rooms.Web;

[Route("/api/rooms")]
public class RoomController : ControllerBase
{

    public static Room Room { get; }

    static RoomController()
    {
        Room = new Room();
        _ = Room.RunAsync();
    }

    [HttpGet]
    public async Task ConnectAsync()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var wsocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var socket = new RoomSocket(wsocket);
            await Room.ListenAsync(socket);
        }
    }

}