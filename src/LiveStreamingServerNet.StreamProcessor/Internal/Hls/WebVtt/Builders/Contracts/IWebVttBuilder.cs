namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt.Builders.Contracts
{
    internal interface IWebVttBuilder
    {
        IWebVttBuilder AddCue(SubtitleCue cue);
        string Build();
    }
}
