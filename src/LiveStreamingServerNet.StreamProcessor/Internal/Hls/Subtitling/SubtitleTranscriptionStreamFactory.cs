using LiveStreamingServerNet.StreamProcessor.Transcriptions;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling
{
    internal record SubtitleTranscriptionStreamFactory(
        SubtitleTrackOptions Options,
        ITranscriptionStreamFactory Factory
    );
}
