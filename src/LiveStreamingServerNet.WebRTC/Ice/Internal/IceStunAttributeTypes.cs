namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    public static class IceStunAttributeTypes
    {
        public static class ComprehensionRequired
        {
            public const ushort Priority = 0x0024;
            public const ushort UseCandidate = 0x0025;

            public static readonly IReadOnlyList<ushort> All =
            [
                Priority,
                UseCandidate
            ];
        }

        public static class ComprehensionOptional
        {
            public const ushort IceControlled = 0x8029;
            public const ushort IceControlling = 0x802A;

            public static readonly IReadOnlyList<ushort> All =
            [
                IceControlled,
                IceControlling
            ];
        }
    }
}
