using System.Text;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Tests
{
    public class RoomStreamTests
    {

        [Fact]
        public async void TestRoomStreamWriteProtocol()
        {
            var protocol = new RoomProtocol();
            var content = Encoding.UTF8.GetBytes("CONTENT");
            using (var stream = new FileRoomStream("stream.txt", FileMode.Create))
            {
                protocol.Verb = RoomVerb.Parse("VERB ");
                protocol.Channel = RoomChannel.Parse("0000 ");
                protocol.Count = RoomCount.Parse($"{content.Length}\n");
                protocol.Content = RoomContent.Create(content);
                await stream.WriteProtocolAsync(protocol);
            }
        }

        [Fact]
        public async void TestRoomStreamWriteMessage()
        {
            var message = new RoomMessage();
            var content = Encoding.UTF8.GetBytes("CONTENT");
            using (var stream = new FileRoomStream("stream.txt", FileMode.Create))
            {
                message.Verb = "SOME_VERB";
                message.Channel = -1;
                message.Content = content;
                await stream.WriteMessageAsync(message);
            }
        }

        [Fact]
        public async void TestRoomStreamReadProtocol()
        {
            var protocol = new RoomProtocol();
            var content = "";
            using (var stream = new FileRoomStream("stream.txt", FileMode.Open))
            {
                protocol.Verb = default;
                protocol.Channel = default;
                protocol.Content = default;
                await stream.ReadProtocolAsync(protocol);
                content = Encoding.UTF8.GetString(protocol.Content.Data!);
            }
        }

        [Fact]
        public async void TestRoomStreamReadMessage()
        {
            var message = new RoomMessage();
            var content = "";
            using (var stream = new FileRoomStream("stream.txt", FileMode.Open))
            {
                message.Verb = string.Empty;
                message.Channel = 0;
                message.Content = Array.Empty<byte>();
                await stream.ReadMessageAsync(message);
                content = Encoding.UTF8.GetString(message.Content);
            }
        }

        public class FileRoomStream : RoomStream
        {

            public FileStream Stream { get; }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token)
            {
                var result = await Stream.ReadAsync(buffer, token);
                return result;
            }

            public override async ValueTask<int> WriteAsync(Memory<byte> buffer, CancellationToken token)
            {
                await Stream.WriteAsync(buffer, token);
                return buffer.Length;
            }

            protected override async ValueTask DisposeAsync(bool disposing)
            {
                if (disposing) await Stream.DisposeAsync();
                await base.DisposeAsync(disposing);
            }

            public FileRoomStream(string path, FileMode mode) : base(new byte[4]) => Stream = new FileStream(path, mode);

        }

    }
}