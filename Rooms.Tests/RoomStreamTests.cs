using System.Text;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Tests
{
    public class RoomStreamTests
    {

        [Fact]
        public async void TestRoomStreamWrite()
        {
            var message = new RoomMessage();
            using (var stream = new FileRoomStream("stream.txt", FileMode.Create))
            {
                message.Verb = RoomVerb.Parse("VERB ");
                message.Channel = RoomChannel.Parse("0000 ");
                message.Content = RoomContent.Create(Encoding.UTF8.GetBytes("CONTENT"));
                await stream.WriteMessageAsync(message);
            }
        }

        [Fact]
        public async void TestRoomStreamRead()
        {
            var message = new RoomMessage();
            using (var stream = new FileRoomStream("stream.txt", FileMode.Open))
            {
                message.Verb = default;
                message.Channel = default;
                message.Content = default;
                await stream.ReadMessageAsync(message);
            }
        }

        public class FileRoomStream : RoomStream
        {

            public FileStream Stream { get; }

            protected override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token)
            {
                var result = await Stream.ReadAsync(buffer, token);
                return result;
            }

            protected override async ValueTask<int> WriteAsync(Memory<byte> buffer, CancellationToken token)
            {
                await Stream.WriteAsync(buffer, token);
                return buffer.Length;
            }

            protected override async ValueTask DisposeAsync(bool disposing)
            {
                if (disposing) await Stream.DisposeAsync();
                await base.DisposeAsync(disposing);
            }

            public FileRoomStream(string path, FileMode mode) : base(new byte[1024]) => Stream = new FileStream(path, mode);

        }

    }
}