using System;
using System.Globalization;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomCount
    {

        public readonly byte[] Data;
        public int Length => Data?.Length ?? 0;
        public override string ToString() => $"{Encoding.UTF8.GetString(Data)}";
        public bool Validate() => Verify(Data);
        public RoomCount(byte[] data) => Data = data;



        public static bool Verify(ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
                return false;
            int index;
            if ((index = data.Slice(0).ScanDigit()) < 1)
                return false;
            if (data.Slice(index).ScanBlanks() < 1)
                return false;
            return true;
        }

        public static bool Verify(ReadOnlySpan<char> data)
        {
            if (data.Length == 0)
                return false;
            int index;
            if ((index = data.Slice(0).ScanDigit()) < 1)
                return false;
            if (data.Slice(index).ScanBlanks() < 1)
                return false;
            return true;
        }

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
            if (number < 0) throw new InvalidCastException("Negative values are not allowed");
            var text = $"{number}\n";
            var count = new RoomCount(Encoding.UTF8.GetBytes(text));
            return count;
        }

        public static explicit operator RoomCount(long number)
        {
            if (number < 0) throw new InvalidCastException("Negative values are not allowed");
            var text = $"{number}\n";
            var count = new RoomCount(Encoding.UTF8.GetBytes(text));
            return count;
        }

        public static explicit operator long(RoomCount count)
        {
            if (count.Length >= 2)
            {
                var text = Encoding.UTF8.GetString(count.Data);
                var number = long.Parse(text, NumberStyles.Integer, null);
                return number;
            }
            throw new InvalidOperationException("Invalid internal data");
        }

    }

}