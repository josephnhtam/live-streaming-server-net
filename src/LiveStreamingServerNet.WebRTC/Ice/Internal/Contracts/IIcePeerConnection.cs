namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IIcePeerConnection : IDisposable
    {
        bool SendPacket(ReadOnlyMemory<byte> buffer);
        event EventHandler<IcePacketEventArgs> OnPacketReceived;
    }
}
