using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Protocol
{
    public static class RoomContentUtils
    {
        public static async ValueTask<string> ReadAsTextAsync(this Stream content, Encoding? encoding = null, CancellationToken token = default)
        {
            var reader = new StreamReader(content, encoding ?? Encoding.UTF8);
            var text = await reader.ReadToEndAsync();
            return text;
        }

        public static async ValueTask<T?> ReadAsJsonAsync<T>(this Stream content, JsonSerializerOptions? options = null, CancellationToken token = default)
        {
            var json = await JsonSerializer.DeserializeAsync<T>(content, options, token);
            return json;
        }

        public static async ValueTask<FileStream> ReadAsFileAsync(this Stream content, string path)
        {
            var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            await content.CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static ValueTask<Stream> CreateAsTextAsync(string text, Encoding? encoding = null, CancellationToken token = default)
        {
            encoding ??= Encoding.UTF8;
            var stream = new MemoryStream(encoding.GetBytes(text));
            stream.Seek(0, SeekOrigin.Begin);
            return ValueTask.FromResult<Stream>(stream);
        }

        public static async ValueTask<Stream> CreateAsJsonAsync<T>(T? json, JsonSerializerOptions? options = null, CancellationToken token = default)
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, json, options, token);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static ValueTask<Stream> CreateAsFileAsync(string path, CancellationToken token = default)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            return ValueTask.FromResult<Stream>(stream);
        }
    }
}