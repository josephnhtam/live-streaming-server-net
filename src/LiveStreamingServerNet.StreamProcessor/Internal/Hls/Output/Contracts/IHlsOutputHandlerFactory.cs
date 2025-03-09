using static LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.HlsTransmuxer;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts
{
    internal interface IHlsOutputHandlerFactory
    {
        IHlsOutputHandler Create(Configuration config);
    }
}
