namespace LiveStreamingServerNet.AdminPanelUI.Dtos
{
    public class StreamDto
    {
        public required string Id { get; set; }
        public required uint ClientId { get; set; }
        public required string StreamPath { get; set; }
        public required int SubscribersCount { get; set; }
        public required DateTime StartTime { get; set; }
        public required IReadOnlyDictionary<string, string> StreamArguments { get; set; }

        public int VideoCodecId { get; set; } = default!;
        public int Height { get; set; }
        public int Width { get; set; }
        public int Framerate { get; set; }

        public int AudioCodecId { get; set; } = default!;
        public int AudioSampleRate { get; set; }
        public int AudioChannels { get; set; }
    }
}
