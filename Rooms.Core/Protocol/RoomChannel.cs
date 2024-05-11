using System;
using System.Globalization;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomChannel
    {

        public readonly ArraySegment<byte> Data;
        public int Length => Data.Count;
        public override string ToString() => $"{Encoding.UTF8.GetString(Data)}";
        public RoomChannel(ArraySegment<byte> data) => Data = data;


        public static int Scan(ReadOnlySpan<byte> data, int index = 0)
        {
            if (index < data.Length && CheckSign(data[index]))
                index++;
            while (index < data.Length && CheckHexadecimal(data[index]))
                index++;
            if (index < data.Length && CheckBlank(data[index]))
                index++;
            return index;
            static bool CheckSign(byte c) => c == '+' || c == '-';
            static bool CheckHexadecimal(byte c) => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9';
            static bool CheckBlank(byte c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        public static int Scan(ReadOnlySpan<char> data, int index = 0)
        {
            if (index < data.Length && CheckSign(data[index]))
                index++;
            while (index < data.Length && CheckHexadecimal(data[index]))
                index++;
            if (index < data.Length && CheckBlank(data[index]))
                index++;
            return index;
            static bool CheckSign(char c) => c == '+' || c == '-';
            static bool CheckHexadecimal(char c) => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9';
            static bool CheckBlank(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';
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

        public static explicit operator RoomChannel(int number)
        {
            var text = number.ToString("x");
            var channel = new RoomChannel(Encoding.UTF8.GetBytes(text));
            return channel;
        }

        public static explicit operator int(RoomChannel channel)
        {
            if (channel.Length == 0) return 0;
            if (channel.Length == 1)
                if (channel.Data[0] == '+') return 0;
                else if (channel.Data[0] == '-') return -1;
            var text = Encoding.UTF8.GetString(channel.Data);
            var number = int.Parse(text, NumberStyles.HexNumber);
            return number;
        }

    }

}