using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Utilities.Mediators.Internal
{
    internal abstract class RequestHandlerWrapper { }

    internal abstract class RequestHandlerWrapper<TResponse> : RequestHandlerWrapper
    {
        public abstract ValueTask<TResponse> HandleAsync(IRequest<TResponse> request, CancellationToken cancellationToken);
    }

    internal class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapper<TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _handler;

        public RequestHandlerWrapper(IServiceProvider services)
        {
            _handler = services.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        }

        public override ValueTask<TResponse> HandleAsync(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            return HandleAsync((TRequest)request, cancellationToken);
        }

        public ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            return _handler.Handle(request, cancellationToken);
        }
    }

}
