using LiveStreamingServerNet.Utilities.Mediators.Internal.Contracts;

namespace LiveStreamingServerNet.Utilities.Mediators.Internal
{
    internal class RequestHandlerWrapperMap : IRequestHandlerWrapperMap
    {
        private readonly IReadOnlyDictionary<Type, Type> _wrapperMap;

        public RequestHandlerWrapperMap(IReadOnlyDictionary<Type, Type> wrapperMap)
        {
            _wrapperMap = new Dictionary<Type, Type>(wrapperMap);
        }

        public IReadOnlyDictionary<Type, Type> GetRequestHandlerWrappers()
        {
            return _wrapperMap;
        }
    }
}
