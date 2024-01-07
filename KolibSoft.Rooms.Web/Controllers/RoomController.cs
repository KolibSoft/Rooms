using Microsoft.AspNetCore.Mvc;

namespace KolibSoft.Rooms.Web.Controllers;


[Route("/api/rooms")]
public class RoomController : ControllerBase
{

    [HttpGet]
    public async Task ConnectAsync()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var wsocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var socket = new RoomSocket(wsocket);
            await Room.Shared.Listen(socket);
        }
    }

}