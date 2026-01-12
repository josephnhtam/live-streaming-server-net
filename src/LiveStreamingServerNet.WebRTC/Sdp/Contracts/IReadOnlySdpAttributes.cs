using LiveStreamingServerNet.WebRTC.Sdp.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Sdp.Contracts
{
    public interface IReadOnlySdpAttributes : IReadOnlyList<ISdpAttribute>
    {
        string? GetAttributeValue(string name);
        IEnumerable<ISdpAttribute> GetAttributes(string name);
        T? GetAttribute<T>(string name) where T : class, ISdpAttribute;
        IEnumerable<T> GetAttributes<T>(string name) where T : class, ISdpAttribute;
    }
}
