using System.Reflection;

namespace LiveStreamingServerNet.Utilities.Mediators.Contracts
{
    public interface IMediatorConfigurator
    {
        void AddRequestHandler<TRequestHandler, TRequest, TResponse>()
            where TRequestHandler : IRequestHandler<TRequest, TResponse>
            where TRequest : IRequest<TResponse>;

        void AddRequestHandlerFromAssembly(params Assembly[] assemblies);
    }
}
