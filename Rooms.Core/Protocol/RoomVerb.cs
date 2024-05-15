using System;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomVerb
    {

        public ReadOnlyMemory<byte> Data => _data;
        public int Length => _data.Count;
        public override string ToString() => $"{Encoding.UTF8.GetString(_data)}";
        public bool Validate() => Verify(_data);
        public RoomVerb(ArraySegment<byte> data) => _data = data;
        private readonly ArraySegment<byte> _data;

        public static bool Verify(ReadOnlySpan<byte> data)
        {
            if (data.Length < 1) return false;
            var index = data.Slice(0).ScanWord();
            return index == data.Length;
        }

        public static bool Verify(ReadOnlySpan<char> data)
        {
            if (data.Length < 1) return false;
            var index = data.Slice(0).ScanWord();
            return index == data.Length;
        }

        public static bool TryParse(ReadOnlySpan<byte> data, out RoomVerb verb)
        {
            if (Verify(data))
            {
                verb = new RoomVerb(data.ToArray());
                return true;
            }
            verb = default;
            return false;
        }

        public static bool TryParse(ReadOnlySpan<char> data, out RoomVerb verb)
        {
            if (Verify(data))
            {
                verb = new RoomVerb(Encoding.UTF8.GetBytes(new string(data)));
                return true;
            }
            verb = default;
            return false;
        }

        public static RoomVerb Parse(ReadOnlySpan<byte> data)
        {
            if (TryParse(data, out RoomVerb verb)) return verb;
            throw new FormatException($"Room verb format is incorrect: {Encoding.UTF8.GetString(data)}");
        }

        public static RoomVerb Parse(ReadOnlySpan<char> data)
        {
            if (TryParse(data, out RoomVerb verb)) return verb;
            throw new FormatException($"Room verb format is incorrect: {new string(data)}");
        }

    }

}