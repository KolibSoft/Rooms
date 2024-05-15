using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Streams
{
    public static class RoomStreamUtils
    {

        public static async Task<string> ReadContentAsText(this IRoomStream stream, long count, Encoding? encoding = null, CancellationToken token = default)
        {
            encoding ??= Encoding.UTF8;
            var content = new MemoryStream();
            await stream.ReadContentAsync(count, content, token);
            var text = encoding.GetString(content.ToArray());
            return text;
        }

        public static async Task<T?> ReadContentAsJson<T>(this IRoomStream stream, long count, JsonSerializerOptions? options = null, CancellationToken token = default)
        {
            var content = new MemoryStream();
            await stream.ReadContentAsync(count, content, token);
            var json = await JsonSerializer.DeserializeAsync<T>(content, options, token);
            return json;
        }

        public static async Task WriteContentAsText(this IRoomStream stream, string text, Encoding? encoding = null, CancellationToken token = default)
        {
            encoding ??= Encoding.UTF8;
            var content = new MemoryStream(encoding.GetBytes(text));
            await stream.WriteContentAsync(content.Length, content, token);
        }

        public static async Task WriteContentAsJson<T>(this IRoomStream stream, T? json, JsonSerializerOptions? options = null, CancellationToken token = default)
        {
            var content = new MemoryStream();
            await JsonSerializer.SerializeAsync<T?>(content, json, options, token);
            content.Seek(0, SeekOrigin.Begin);
            await stream.WriteContentAsync(content.Length, content, token);
        }

    }
}