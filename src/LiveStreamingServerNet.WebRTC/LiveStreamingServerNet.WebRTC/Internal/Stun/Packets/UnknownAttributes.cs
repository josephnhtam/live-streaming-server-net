namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal class UnknownAttributes : List<ushort>
    {
        public bool HasUnknownComprehensionRequiredAttributes()
            => this.Any(StunAttributeType.ComprehensionRequired.InRange);

        public IEnumerable<ushort> UnknownComprehensionRequiredAttributes()
            => this.Where(StunAttributeType.ComprehensionRequired.InRange);

        public IEnumerable<ushort> UnknownComprehensionOptionalAttributes()
            => this.Where(static t => !StunAttributeType.ComprehensionRequired.InRange(t));
    }
}
