using System;
using System.Globalization;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public struct RoomChannel
    {

        public ReadOnlyMemory<byte> Data => _data;

        public override string ToString() => $"{Encoding.UTF8.GetString(_data)}";

        public RoomChannel(ArraySegment<byte> data) => _data = data;

        private ArraySegment<byte> _data;

        public static int Scan(ReadOnlySpan<byte> data, int index = 0)
        {
            if (index < data.Length && CheckSign(data[index]))
                index++;
            while (index < data.Length && CheckHexadecimal(data[index]))
                index++;
            return index;
            static bool CheckSign(byte c) => c == '+' || c == '-';
            static bool CheckHexadecimal(byte c) => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9';
        }

        public static int Scan(ReadOnlySpan<char> data, int index = 0)
        {
            if (index < data.Length && CheckSign(data[index]))
                index++;
            while (index < data.Length && CheckHexadecimal(data[index]))
                index++;
            return index;
            static bool CheckSign(char c) => c == '+' || c == '-';
            static bool CheckHexadecimal(char c) => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9';
        }

        public static bool Verify(ReadOnlySpan<byte> data) => Scan(data) == data.Length;
        public static bool Verify(ReadOnlySpan<char> data) => Scan(data) == data.Length;

        public static bool TryParse(ReadOnlySpan<byte> data, out RoomChannel channel)
        {
            if (Verify(data))
            {
                channel = new RoomChannel(data.ToArray());
                return true;
            }
            channel = default;
            return false;
        }

        public static bool TryParse(ReadOnlySpan<char> data, out RoomChannel channel)
        {
            if (Verify(data))
            {
                channel = new RoomChannel(Encoding.UTF8.GetBytes(new string(data)));
                return true;
            }
            channel = default;
            return false;
        }

        public static RoomChannel Parse(ReadOnlySpan<byte> data)
        {
            if (TryParse(data, out RoomChannel channel)) return channel;
            throw new FormatException($"Room channel format is incorrect: {Encoding.UTF8.GetString(data)}");
        }

        public static RoomChannel Parse(ReadOnlySpan<char> data)
        {
            if (TryParse(data, out RoomChannel channel)) return channel;
            throw new FormatException($"Room channel format is incorrect: {new string(data)}");
        }

        public static explicit operator RoomChannel(long number)
        {
            var text = number.ToString("x");
            var channel = new RoomChannel(Encoding.UTF8.GetBytes(text));
            return channel;
        }

        public static explicit operator long(RoomChannel channel)
        {
            if (channel._data.Count == 0) return 0;
            if (channel._data.Count == 1)
                if (channel._data[0] == '+') return 0;
                else if (channel._data[0] == '-') return -1;
            var text = Encoding.UTF8.GetString(channel._data);
            var number = long.Parse(text, NumberStyles.HexNumber);
            return number;
        }

    }

}