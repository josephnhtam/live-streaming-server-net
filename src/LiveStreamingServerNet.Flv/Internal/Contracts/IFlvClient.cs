namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvClient : IAsyncDisposable
    {
        string ClientId { get; }
        string StreamPath { get; }
        CancellationToken StoppingToken { get; }
        IFlvWriter FlvWriter { get; }
        void Initialize(string clientId, string streamPath, IStreamWriter streamWriter, CancellationToken stoppingToken);
        void Stop();
        void CompleteInitialization();
        Task UntilIntializationComplete();
        Task UntilComplete();
    }
}
