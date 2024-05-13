using System;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomVerb
    {

        public readonly byte[] Data;
        public int Length => Data?.Length ?? 0;
        public override string ToString() => $"{Encoding.UTF8.GetString(Data)}";
        public bool Validate() => Verify(Data);
        public RoomVerb(byte[] data) => Data = data;

        public static bool Verify(ReadOnlySpan<byte> data)
        {
            if (data.Length < 2) return false;
            var index = data.Slice(0).ScanWord();
            if (index < 1) return false;
            if (index < data.Length && DataUtils.IsBlank(data[index]))
                index++;
            return index == data.Length;
        }

        public static bool Verify(ReadOnlySpan<char> data)
        {
            if (data.Length < 2) return false;
            var index = data.Slice(0).ScanWord();
            if (index < 1) return false;
            if (index < data.Length && DataUtils.IsBlank(data[index]))
                index++;
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