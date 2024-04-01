using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Sockets;

namespace KolibSoft.Rooms.Core.Services
{

    /// <summary>
    /// Delegates the creation of a Room socket.
    /// </summary>
    /// <param name="connstring">Implementation specific connection string.</param>
    /// <returns></returns>
    public delegate Task<IRoomSocket> RoomConnector(string connstring);

}