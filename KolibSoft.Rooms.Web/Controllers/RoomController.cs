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
            var socket = new RoomSocket(await HttpContext.WebSockets.AcceptWebSocketAsync());
            while (socket.IsAlive)
            {
                var message = await socket.ReceiveAsync();
                if (message != null)
                    await socket.SendAsync(message);
            }
        }
    }

}