using System.Text;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Streams;

namespace KolibSoft.Rooms.Tests
{
    public class RoomStreamTests
    {

        [Fact]
        public async void TestRoomStreamWriteMessage()
        {
            var content = Encoding.UTF8.GetBytes("SOME CONTENT");
            using (var stream = new FileRoomStream("stream.txt", FileMode.Create))
            {
                var message = new RoomMessage
                {
                    Verb = "VERB",
                    Channel = 0,
                    Content = new MemoryStream(content)
                };
                await stream.WriteMessageAsync(message);
            }
        }

        [Fact]
        public async void TestRoomStreamReadMessage()
        {
            var content = "";
            using (var stream = new FileRoomStream("stream.txt", FileMode.Open))
            {
                var message = await stream.ReadMessageAsync();
                var memory = new MemoryStream();
                await message.Content.CopyToAsync(memory);
                memory.Seek(0, SeekOrigin.Begin);
                content = Encoding.UTF8.GetString(memory.ToArray());
            }
        }

        public class FileRoomStream : RoomStream
        {

            public FileStream Stream { get; }
            public override bool IsAlive => Stream.SafeFileHandle?.IsInvalid == false;

            protected override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token)
            {
                var result = await Stream.ReadAsync(buffer, token);
                return result;
            }

            protected override async ValueTask<int> WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token)
            {
                await Stream.WriteAsync(buffer, token);
                return buffer.Length;
            }

            protected override async ValueTask DisposeAsync(bool disposing)
            {
                if (disposing) await Stream.DisposeAsync();
                await base.DisposeAsync(disposing);
            }

            public FileRoomStream(string path, FileMode mode) : base(new byte[1024], new byte[1024]) => Stream = new FileStream(path, mode);

        }

    }
}