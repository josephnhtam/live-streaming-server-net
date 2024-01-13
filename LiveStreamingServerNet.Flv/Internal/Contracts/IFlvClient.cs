namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvClient : IAsyncDisposable
    {
        uint ClientId { get; }
        string StreamPath { get; }
        CancellationToken StoppingToken { get; }
        IFlvWriter FlvWriter { get; }
        void Initialize(uint clientId, string streamPath, IStreamWriter streamWriter, CancellationToken stoppingToken);
        void Stop();
        void CompleteInitialization();
        Task UntilIntializationComplete();
        Task UntilComplete();
    }
}
