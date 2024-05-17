using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Protocol
{
    public sealed class RoomMessage
    {
        public string Verb { get; set; } = string.Empty;
        public int Channel { get; set; } = default;
        public Stream Content { get; set; } = Stream.Null;
    }
}