using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts
{
    internal interface IStunAttribute
    {
        ushort Type { get; }
        void WriteValue(BindingRequest request, IDataBuffer buffer);
    }
}
