using LiveStreamingServerNet.StreamProcessor.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts
{
    public interface ITranscriptionStreamFactory
    {
        ITranscriptionStream Create(IMediaStreamWriterFactory inputStreamWriterFactory, SubtitleTrackOptions options);
    }
}
