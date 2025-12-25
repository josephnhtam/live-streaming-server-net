namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal static class StunTypeHelper
    {
        public static ushort CreateType(ushort method, StunClass msgClass)
        {
            var m_0_3 = (ushort)(method & 0x000F);
            var m_4_6 = (ushort)((method & 0x0070) << 1);
            var m_7_11 = (ushort)((method & 0x0F80) << 2);

            var c = (ushort)msgClass;
            var c0 = (ushort)((c & 0x01) << 4);
            var c1 = (ushort)((c & 0x02) << 7);

            return (ushort)(m_0_3 | m_4_6 | m_7_11 | c0 | c1);
        }

        public static (ushort Method, StunClass Class) GetMethodAndClass(ushort type)
        {
            var m_0_3 = (ushort)(type & 0x000F);
            var m_4_6 = (ushort)((type & 0x00E0) >> 1);
            var m_7_11 = (ushort)((type & 0x3E00) >> 2);
            var method = (ushort)(m_0_3 | m_4_6 | m_7_11);

            var c0 = (type & 0x0010) >> 4;
            var c1 = (type & 0x0100) >> 7;
            var msgClass = (StunClass)(c0 | c1);

            return (method, msgClass);
        }
    }
}
