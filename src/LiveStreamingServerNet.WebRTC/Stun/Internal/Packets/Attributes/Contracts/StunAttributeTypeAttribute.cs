namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    internal class StunAttributeTypeAttribute : Attribute
    {
        public readonly ushort Type;

        public StunAttributeTypeAttribute(ushort type)
        {
            Type = type;
        }
    }
}
