namespace LiveStreamingServerNet.Utilities.Mediators.Internal
{
    internal record struct MediatorConfiguration(IDictionary<Type, RequestRelevantTypes> RequestRelavantTypesMap);
    internal record struct RequestRelevantTypes(Type ResponseType, Type HandlerType, Type WrapperType);
}
