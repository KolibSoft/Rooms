namespace KolibSoft.Rooms.Core
{
    public sealed class RoomMessage<T>
    {
        public string Verb { get; set; } = string.Empty;
        public int Channel { get; set; } = 0;
        public T? Content { get; set; } = default;
    }
}