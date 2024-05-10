using System;
using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public struct RoomContent
    {

        public ReadOnlySpan<byte> Data => _data;

        public override string ToString() => $"{Convert.ToBase64String(Data)}";

        public RoomContent(ArraySegment<byte> data) => _data = data;

        private ArraySegment<byte> _data;

        public static RoomContent Create(ReadOnlySpan<byte> data) => new RoomContent(data.ToArray());
        public static RoomContent Create(ReadOnlySpan<char> data) => new RoomContent(Convert.FromBase64String(new string(data)));

    }

}