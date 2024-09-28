namespace LiveStreamingServerNet.Rtmp.Client.Contracts
{
    public interface IRtmpStream
    {
        public uint StreamId { get; }

        void Play(string streamName);
        void Play(string streamName, double start, double duration, bool reset);
    }
}
