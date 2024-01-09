namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvClient
    {
        IClientStreamWriter StreamWriter { get; }
    }
}
