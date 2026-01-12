namespace LiveStreamingServerNet.WebRTC.Sdp.Attributes.Contracts
{
    public interface ISdpAttribute
    {
        string Name { get; }
        string? Value { get; }
    }
}
