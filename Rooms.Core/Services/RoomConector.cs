using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Sockets;

namespace KolibSoft.Rooms.Core.Services
{

    /// <summary>
    /// Socket implementation connection delegate.
    /// </summary>
    /// <param name="connstring">Implementation specific connection string.</param>
    /// <returns>Specific socket implementation.</returns>
    public delegate Task<IRoomSocket> RoomConnector(string connstring);
    
}