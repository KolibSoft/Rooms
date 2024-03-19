using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// Socket implementation connection delegate.
    /// </summary>
    /// <param name="server">Implementation server identifier.</param>
    /// <returns>Specific socket implementation.</returns>
    public delegate Task<IRoomSocket> RoomConnector(string server);
    
}