using System;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public readonly struct RoomVerb
    {

        public readonly byte[] Data;
        public int Length => Data?.Length ?? 0;
        public override string ToString() => $"{Encoding.UTF8.GetString(Data)}";
        public RoomVerb(byte[] data) => Data = data;

        public static int Scan(ReadOnlySpan<byte> data, int index = 0)
        {
            while (index < data.Length && CheckWord(data[index]))
                index++;
            if (index < data.Length && CheckBlank(data[index]))
                index++;
            return index;
            static bool CheckWord(byte c) => c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
            static bool CheckBlank(byte c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        public static int Scan(ReadOnlySpan<char> data, int index = 0)
        {
            while (index < data.Length && CheckWord(data[index]))
                index++;
            if (index < data.Length && CheckBlank(data[index]))
                index++;
            return index;
            static bool CheckWord(char c) => c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
            static bool CheckBlank(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        public static bool Verify(ReadOnlySpan<byte> data)
        {
            return Scan(data) == data.Length && CheckBlank(data[^1]);
            static bool CheckBlank(byte c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        public static bool Verify(ReadOnlySpan<char> data)
        {
            return Scan(data) == data.Length && CheckBlank(data[^1]);
            static bool CheckBlank(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';
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