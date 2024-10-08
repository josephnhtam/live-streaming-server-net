namespace LiveStreamingServerNet.Utilities.Mediators.Internal.Contracts
{
    internal interface IRequestHandlerWrapperMap
    {
        IReadOnlyDictionary<Type, Type> GetRequestHandlerWrappers();
    }
}
