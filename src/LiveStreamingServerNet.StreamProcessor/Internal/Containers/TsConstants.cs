namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
{
    internal static class TsConstants
    {
        public const int TsPacketSize = 188;

        public const ushort ProgramAssociationPID = 0x0;
        public const byte ProgramAssociationTableID = 0;

        public const ushort ProgramMapPID = 0x1000;
        public const byte ProgramMapTableID = 2;

        public const ushort TransportStreamIdentifier = 1;
        public const ushort ProgramNumber = 1;

        public const ushort VideoPID = 0x100;
        public const byte VideoSID = 0xE0;
        public const byte AVCStreamType = 0x1B;

        public const ushort AudioPID = 0x101;
        public const byte AudioSID = 0xC0;
        public const byte AACStreamType = 0x0F;

        public const byte SyncByte = 0x47;
        public const byte StuffingByte = 0xFF;
    }
}
