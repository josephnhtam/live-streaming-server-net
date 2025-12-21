namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class StunAttributeTypeAttribute : Attribute
    {
        public readonly ushort Type;

        public StunAttributeTypeAttribute(ushort type)
        {
            Type = type;
        }
    }
}
