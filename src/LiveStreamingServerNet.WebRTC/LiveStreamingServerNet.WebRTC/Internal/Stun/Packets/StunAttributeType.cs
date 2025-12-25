namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal static class StunAttributeType
    {
        public static class ComprehensionRequired
        {
            public const ushort MappedAddress = 0x0001;
            public const ushort Username = 0x0006;
            public const ushort MessageIntegrity = 0x0008;
            public const ushort ErrorCode = 0x0009;
            public const ushort UnknownAttributes = 0x000A;
            public const ushort Realm = 0x0014;
            public const ushort Nonce = 0x0015;
            public const ushort MessageIntegritySha256 = 0x001C;
            public const ushort PasswordAlgorithm = 0x001D;
            public const ushort Userhash = 0x001E;
            public const ushort XorMappedAddress = 0x0020;

            public static readonly IReadOnlyList<ushort> All =
            [
                MappedAddress,
                Username,
                MessageIntegrity,
                ErrorCode,
                UnknownAttributes,
                Realm,
                Nonce,
                MessageIntegritySha256,
                PasswordAlgorithm,
                Userhash,
                XorMappedAddress
            ];

            public static bool InRange(ushort type) => type <= 0x7FFF;
        }

        public static class ComprehensionOptional
        {
            public const ushort PasswordAlgorithms = 0x8002;
            public const ushort AlternateDomain = 0x8003;
            public const ushort Software = 0x8022;
            public const ushort AlternateServer = 0x8023;
            public const ushort Fingerprint = 0x8028;

            public static readonly IReadOnlyList<ushort> All =
            [
                PasswordAlgorithms,
                AlternateDomain,
                Software,
                AlternateServer,
                Fingerprint
            ];
        }
    }
}
