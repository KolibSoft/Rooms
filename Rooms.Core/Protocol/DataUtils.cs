using System;

namespace KolibSoft.Rooms.Core.Protocol
{
    public static class DataUtils
    {

        public static bool IsBlank(byte c) => c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f';
        public static int ScanBlanks(this ReadOnlySpan<byte> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsBlank(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        public static bool IsBlank(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f';
        public static int ScanBlanks(this ReadOnlySpan<char> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsBlank(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        public static bool IsLetter(byte c) => c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
        public static int ScanWord(this ReadOnlySpan<byte> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsLetter(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        public static bool IsLetter(char c) => c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
        public static int ScanWord(this ReadOnlySpan<char> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsLetter(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        public static bool IsSign(byte c) => c == '-' || c == '+';
        public static bool IsDigit(byte c) => c >= '0' && c <= '9';
        public static int ScanDigit(this ReadOnlySpan<byte> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsDigit(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        public static bool IsSign(char c) => c == '-' || c == '+';
        public static bool IsDigit(char c) => c >= '0' && c <= '9';
        public static int ScanDigit(this ReadOnlySpan<char> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsDigit(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        public static bool IsHexadecimal(byte c) => c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';
        public static int ScanHexadecimal(this ReadOnlySpan<byte> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsHexadecimal(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        public static bool IsHexadecimal(char c) => c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';
        public static int ScanHexadecimal(this ReadOnlySpan<char> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsHexadecimal(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

    }
}