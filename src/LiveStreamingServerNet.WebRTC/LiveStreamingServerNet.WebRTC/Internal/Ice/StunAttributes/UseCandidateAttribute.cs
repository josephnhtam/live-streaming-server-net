using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Ice.StunAttributes
{
    [StunAttributeType(IceStunAttributeType.ComprehensionRequired.UseCandidate)]
    internal record UseCandidateAttribute() : IStunAttribute
    {
        public ushort Type => IceStunAttributeType.ComprehensionRequired.UseCandidate;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer) { }

        public static UseCandidateAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new UseCandidateAttribute();
    }
}
