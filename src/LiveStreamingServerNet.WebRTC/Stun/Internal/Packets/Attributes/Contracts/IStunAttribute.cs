using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts
{
    internal interface IStunAttribute
    {
        ushort Type { get; }
        void WriteValue(TransactionId transactionId, IDataBuffer buffer);
    }
}
