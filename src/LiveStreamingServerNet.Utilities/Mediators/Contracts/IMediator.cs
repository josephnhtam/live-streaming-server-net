namespace LiveStreamingServerNet.Utilities.Mediators.Contracts
{
    public interface IMediator
    {
        ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
