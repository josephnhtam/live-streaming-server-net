using LiveStreamingServerNet.Utilities.Extensions;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using System.Reflection;

namespace LiveStreamingServerNet.Utilities.Mediators.Internal
{
    internal class MediatorConfigurator : IMediatorConfigurator
    {
        private readonly HashSet<Type> _candidates = new HashSet<Type>();

        public void AddRequestHandler<TRequestHandler, TRequest, TResponse>()
            where TRequestHandler : IRequestHandler<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
        {
            _candidates.Add(typeof(TRequestHandler));
        }

        public void AddRequestHandlerFromAssembly(params Assembly[] assemblies)
        {
            var types = assemblies.SelectMany(x => x.GetTypes()).ToArray();

            foreach (var type in types)
                _candidates.Add(type);
        }

        public MediatorConfiguration Build()
        {
            var requestRelavantTypesMap = _candidates
                .Where(type => type.IsClass && !type.IsAbstract && type.CheckGenericTypeDefinition(typeof(IRequestHandler<,>)))
                .Select(handlerType =>
                {
                    var argumentTypes = handlerType.GetGenericArguments(typeof(IRequestHandler<,>));
                    var requestType = argumentTypes[0];
                    var responseType = argumentTypes[1];
                    var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, responseType);

                    return (RequestType: requestType, HandlerType: new RequestRelevantTypes(responseType, handlerType, wrapperType));
                })
                .DistinctBy(x => x.RequestType)
                .ToDictionary(x => x.RequestType, x => x.HandlerType);

            return new MediatorConfiguration(requestRelavantTypesMap);
        }
    }
}
