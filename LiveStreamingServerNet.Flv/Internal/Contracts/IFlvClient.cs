namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvClient : IAsyncDisposable
    {
        IStreamWriter StreamWriter { get; }
        void Start(IStreamWriter streamWriter, CancellationToken stoppingToken);
        void Stop();
        Task UntilComplete();
    }
}
