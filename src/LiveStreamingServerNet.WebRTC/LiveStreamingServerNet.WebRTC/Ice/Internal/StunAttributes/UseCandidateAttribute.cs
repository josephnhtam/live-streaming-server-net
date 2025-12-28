using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.StunAttributes
{
    [StunAttributeType(IceStunAttributeTypes.ComprehensionRequired.UseCandidate)]
    internal record UseCandidateAttribute() : IStunAttribute
    {
        public ushort Type => IceStunAttributeTypes.ComprehensionRequired.UseCandidate;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer) { }

        public static UseCandidateAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new UseCandidateAttribute();
    }
}
