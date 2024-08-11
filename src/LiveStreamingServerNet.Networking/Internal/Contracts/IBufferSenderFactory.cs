namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface IBufferSenderFactory
    {
        IBufferSender Create();
    }
}
