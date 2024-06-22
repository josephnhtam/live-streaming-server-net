namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface IClientBufferSenderFactory
    {
        IClientBufferSender Create(uint clientId);
    }
}
