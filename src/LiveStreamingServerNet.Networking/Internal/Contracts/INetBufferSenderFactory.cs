namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface INetBufferSenderFactory
    {
        INetBufferSender Create(uint clientId);
    }
}
