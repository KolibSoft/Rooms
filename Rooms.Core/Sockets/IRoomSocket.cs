using System;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Sockets
{

    public interface IRoomSocket : IDisposable
    {
        public bool IsAlive { get; }
        public Task SendAsync(RoomMessage message);
        public Task ReceiveAsync(RoomMessage message);
    }

}