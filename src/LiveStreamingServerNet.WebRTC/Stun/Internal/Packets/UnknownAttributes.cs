namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets
{
    internal class UnknownAttributes : List<ushort>
    {
        public bool HasUnknownComprehensionRequiredAttributes()
            => this.Any(StunAttributeTypes.ComprehensionRequired.InRange);

        public IEnumerable<ushort> UnknownComprehensionRequiredAttributes()
            => this.Where(StunAttributeTypes.ComprehensionRequired.InRange);

        public IEnumerable<ushort> UnknownComprehensionOptionalAttributes()
            => this.Where(static t => !StunAttributeTypes.ComprehensionRequired.InRange(t));
    }
}
