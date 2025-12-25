namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Configurations
{
    internal class StunClientConfiguration
    {
        /// <summary>
        /// Initial retransmission timeout in milliseconds.
        /// Default: 500ms.
        /// </summary>
        public int RetransmissionTimeout { get; set; } = 500;

        /// <summary>
        /// Maximum retransmission timeout in milliseconds.
        /// Default: 1600ms.
        /// </summary>
        public int MaxRetransmissionTimeout { get; set; } = 1600;

        /// <summary>
        /// Max retransmissions.
        /// Default: 7.
        /// </summary>
        public int MaxRetransmissions { get; set; } = 7;

        /// <summary>
        /// Transaction timeout factor.
        /// Default: 16.</summary>
        public int TransactionTimeoutFactor { get; set; } = 16;

        /// <summary>
        /// Automatically handle unknown comprehension-required attributes by returning an 420 error response.
        /// Default: true.
        /// </summary>
        public bool HandleUnknownComprehensionRequiredAttributes { get; set; } = true;
    }
}
