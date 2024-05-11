using System;
using System.Globalization;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public struct RoomCount
    {

        public ReadOnlyMemory<byte> Data => _data;

        public override string ToString() => $"{Encoding.UTF8.GetString(_data)}";

        public RoomCount(ArraySegment<byte> data) => _data = data;

        private ArraySegment<byte> _data;

        public static int Scan(ReadOnlySpan<byte> data, int index = 0)
        {
            while (index < data.Length && CheckDigit(data[index]))
                index++;
            return index;
            static bool CheckDigit(byte c) => c >= '0' && c <= '9';
        }

        public static int Scan(ReadOnlySpan<char> data, int index = 0)
        {
            while (index < data.Length && CheckDigit(data[index]))
                index++;
            return index;
            static bool CheckDigit(char c) => c >= '0' && c <= '9';
        }

        public static bool Verify(ReadOnlySpan<byte> data) => Scan(data) == data.Length;
        public static bool Verify(ReadOnlySpan<char> data) => Scan(data) == data.Length;

        public static bool TryParse(ReadOnlySpan<byte> data, out RoomCount count)
        {
            if (Verify(data))
            {
                count = new RoomCount(data.ToArray());
                return true;
            }
            count = default;
            return false;
        }

        public static bool TryParse(ReadOnlySpan<char> data, out RoomCount count)
        {
            if (Verify(data))
            {
                count = new RoomCount(Encoding.UTF8.GetBytes(new string(data)));
                return true;
            }
            count = default;
            return false;
        }

        public static RoomCount Parse(ReadOnlySpan<byte> data)
        {
            if (TryParse(data, out RoomCount count)) return count;
            throw new FormatException($"Room count format is incorrect: {Encoding.UTF8.GetString(data)}");
        }

        public static RoomCount Parse(ReadOnlySpan<char> data)
        {
            if (TryParse(data, out RoomCount count)) return count;
            throw new FormatException($"Room count format is incorrect: {new string(data)}");
        }

        public static explicit operator RoomCount(long number)
        {
            var text = number.ToString("x");
            var count = new RoomCount(Encoding.UTF8.GetBytes(text));
            return count;
        }

        public static explicit operator long(RoomCount count)
        {
            if (count._data.Count == 0) return 0;
            if (count._data.Count == 1)
                if (count._data[0] == '+') return 0;
                else if (count._data[0] == '-') return -1;
            var text = Encoding.UTF8.GetString(count._data);
            var number = long.Parse(text, NumberStyles.HexNumber);
            return number;
        }

    }

}