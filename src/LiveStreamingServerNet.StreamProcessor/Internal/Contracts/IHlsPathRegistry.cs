using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Contracts
{
    internal interface IHlsPathRegistry : IHlsPathMapper
    {
        bool RegisterHlsOutputPath(string streamPath, string outputPath);
        void UnregisterHlsOutputPath(string streamPath);
    }
}
