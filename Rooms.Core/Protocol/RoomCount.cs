using System;
using System.Globalization;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomCount
    {

        public readonly ArraySegment<byte> Data;
        public int Length => Data.Count;
        public override string ToString() => $"{Encoding.UTF8.GetString(Data)}";

        public RoomCount(ArraySegment<byte> data) => Data = data;
        public static int Scan(ReadOnlySpan<byte> data, int index = 0)
        {
            while (index < data.Length && CheckDigit(data[index]))
                index++;
            if (index < data.Length && CheckBlank(data[index]))
                index++;
            return index;
            static bool CheckDigit(byte c) => c >= '0' && c <= '9';
            static bool CheckBlank(byte c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        public static int Scan(ReadOnlySpan<char> data, int index = 0)
        {
            while (index < data.Length && CheckDigit(data[index]))
                index++;
            if (index < data.Length && CheckBlank(data[index]))
                index++;
            return index;
            static bool CheckDigit(char c) => c >= '0' && c <= '9';
            static bool CheckBlank(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';
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

        public static explicit operator RoomCount(int number)
        {
            var text = number.ToString();
            var count = new RoomCount(Encoding.UTF8.GetBytes(text));
            return count;
        }

        public static explicit operator int(RoomCount count)
        {
            var text = Encoding.UTF8.GetString(count.Data);
            var number = int.Parse(text, NumberStyles.Integer);
            return number;
        }

    }

}