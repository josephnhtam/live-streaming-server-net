namespace LiveStreamingServerNet.Utilities.Mediators.Contracts
{
    public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
