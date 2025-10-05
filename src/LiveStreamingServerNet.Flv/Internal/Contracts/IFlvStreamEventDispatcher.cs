namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvServerStreamEventDispatcher
    {
        ValueTask FlvStreamSubscribedAsync(IFlvClient client);
        ValueTask FlvStreamUnsubscribedAsync(IFlvClient client);
    }
}
