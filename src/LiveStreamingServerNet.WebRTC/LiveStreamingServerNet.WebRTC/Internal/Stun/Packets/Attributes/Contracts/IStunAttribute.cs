using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts
{
    internal interface IStunAttribute
    {
        ushort Type { get; }
        void WriteValue(TransactionId transactionId, IDataBuffer buffer);
    }
}
