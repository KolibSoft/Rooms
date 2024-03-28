using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Sockets;

namespace KolibSoft.Rooms.Core.Services
{

    public delegate Task<IRoomSocket> RoomConnector(string connstring);

}