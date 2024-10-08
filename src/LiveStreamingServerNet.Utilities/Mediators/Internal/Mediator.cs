using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Internal.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Utilities.Mediators.Internal
{
    internal class Mediator : IMediator
    {
        private readonly IReadOnlyDictionary<Type, RequestHandlerWrapper> _wrapperCache;

        public Mediator(IServiceProvider services, IEnumerable<IRequestHandlerWrapperMap> wrapperMaps)
        {
            var wrapperMap = wrapperMaps.Select(x => x.GetRequestHandlerWrappers())
                .SelectMany(x => x)
                .DistinctBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            _wrapperCache = CreateWrapperCache(services, wrapperMap);
        }

        public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var wrapper = _wrapperCache.GetValueOrDefault(request.GetType()) as RequestHandlerWrapper<TResponse> ??
                throw new InvalidOperationException($"No handler found for request type {request.GetType()}");

            return wrapper.HandleAsync(request, cancellationToken);
        }

        private Dictionary<Type, RequestHandlerWrapper> CreateWrapperCache(
            IServiceProvider services, IReadOnlyDictionary<Type, Type> wrapperMap)
        {
            var wrapperCache = new Dictionary<Type, RequestHandlerWrapper>();

            foreach (var (requestType, wrapperType) in wrapperMap)
            {
                wrapperCache[requestType] = services.GetRequiredService(wrapperType) as RequestHandlerWrapper ??
                   throw new InvalidOperationException($"No handler wrapper found for request type {requestType}");
            }

            return wrapperCache;
        }
    }
}
