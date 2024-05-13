using System;
using System.Globalization;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomChannel
    {

        public readonly byte[] Data;
        public int Length => Data?.Length ?? 0;
        public override string ToString() => $"{Encoding.UTF8.GetString(Data)}";
        public bool Validate() => Verify(Data);
        public RoomChannel(byte[] data) => Data = data;

        public static bool Verify(ReadOnlySpan<byte> data)
        {
            if (data.Length < 3 || !DataUtils.IsSign(data[0]))
                return false;
            int index;
            if ((index = data.Slice(1).ScanHexadecimal()) < 1)
                return false;
            if (data.Slice(index).ScanBlanks() < 1)
                return false;
            return true;
        }

        public static bool Verify(ReadOnlySpan<char> data)
        {
            if (data.Length < 3 || !DataUtils.IsSign(data[0]))
                return false;
            int index;
            if ((index = data.Slice(1).ScanHexadecimal()) < 1)
                return false;
            if (data.Slice(index).ScanBlanks() < 1)
                return false;
            return true;
        }

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
            if (number >= 0)
            {
                var text = $"+{number:x} ";
                var channel = new RoomChannel(Encoding.UTF8.GetBytes(text));
                return channel;
            }
            else
            {
                var text = $"-{-number:x} ";
                var channel = new RoomChannel(Encoding.UTF8.GetBytes(text));
                return channel;
            }
        }

        public static explicit operator RoomChannel(long number)
        {
            if (number >= 0)
            {
                var text = $"+{number:x} ";
                var channel = new RoomChannel(Encoding.UTF8.GetBytes(text));
                return channel;
            }
            else
            {
                var text = $"-{-number:x} ";
                var channel = new RoomChannel(Encoding.UTF8.GetBytes(text));
                return channel;
            }
        }

        public static explicit operator int(RoomChannel channel)
        {
            if (channel.Length >= 3)
                if (channel.Data[0] == '-')
                {
                    var text = Encoding.UTF8.GetString(channel.Data.AsSpan().Slice(1));
                    var number = int.Parse(text, NumberStyles.HexNumber);
                    return -number;
                }
                else if (channel.Data[0] == '+')
                {
                    var text = Encoding.UTF8.GetString(channel.Data.AsSpan().Slice(1));
                    var number = int.Parse(text, NumberStyles.HexNumber);
                    return number;
                }
            throw new InvalidOperationException("Invalid internal data");
        }

        public static explicit operator long(RoomChannel channel)
        {
            if (channel.Length >= 3)
                if (channel.Data[0] == '-')
                {
                    var text = Encoding.UTF8.GetString(channel.Data.AsSpan().Slice(1));
                    var number = long.Parse(text, NumberStyles.HexNumber);
                    return -number;
                }
                else if (channel.Data[0] == '+')
                {
                    var text = Encoding.UTF8.GetString(channel.Data.AsSpan().Slice(1));
                    var number = long.Parse(text, NumberStyles.HexNumber);
                    return number;
                }
            throw new InvalidOperationException("Invalid internal data");
        }

    }

}