namespace LiveStreamingServerNet.WebRTC.Sdp
{
    public static class SdpNetworkType
    {
        public const string Internet = "IN";
    }

    public static class SdpAddressType
    {
        public const string IPv4 = "IP4";
        public const string IPv6 = "IP6";
    }

    public static class SdpMediaType
    {
        public const string Audio = "audio";
        public const string Video = "video";
        public const string Application = "application";
    }

    public static class SdpProtocol
    {
        public const string UdpTlsRtpSavpf = "UDP/TLS/RTP/SAVPF";
        public const string UdpDtlsSctp = "UDP/DTLS/SCTP";
    }

    public static class SdpAttributeNames
    {
        public const string Candidate = "candidate";
        public const string IceUfrag = "ice-ufrag";
        public const string IcePwd = "ice-pwd";
        public const string IceOptions = "ice-options";
        public const string Fingerprint = "fingerprint";
        public const string Setup = "setup";
        public const string Mid = "mid";
        public const string Rtcp = "rtcp";
        public const string RtcpMux = "rtcp-mux";
        public const string RtcpRsize = "rtcp-rsize";
        public const string RtpMap = "rtpmap";
        public const string Fmtp = "fmtp";
        public const string Ssrc = "ssrc";
        public const string SsrcGroup = "ssrc-group";
        public const string Msid = "msid";
        public const string SctpPort = "sctp-port";
        public const string MaxMessageSize = "max-message-size";
        public const string SendRecv = "sendrecv";
        public const string SendOnly = "sendonly";
        public const string RecvOnly = "recvonly";
        public const string Inactive = "inactive";
        public const string Group = "group";
        public const string Bundle = "BUNDLE";
        public const string ExtMap = "extmap";
        public const string EndOfCandidates = "end-of-candidates";
    }

    public static class SdpSetupRole
    {
        public const string Active = "active";
        public const string Passive = "passive";
        public const string ActPass = "actpass";
    }

    public static class SdpIceOptions
    {
        public const string Trickle = "trickle";
        public const string IceLite = "ice-lite";
        public const string Ice2 = "ice2";
    }
}
