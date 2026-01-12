namespace LiveStreamingServerNet.WebRTC.Stun
{
    public class StunRetransmissionOptions
    {
        /// <summary>
        /// Initial retransmission timeout in milliseconds.
        /// Default: 500ms.
        /// </summary>
        public TimeSpan RetransmissionTimeout { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Maximum retransmission timeout in milliseconds.
        /// Default: 1600ms.
        /// </summary>
        public TimeSpan MaxRetransmissionTimeout { get; set; } = TimeSpan.FromMilliseconds(1600);

        /// <summary>
        /// Max retransmissions.
        /// Default: 14.
        /// </summary>
        public int MaxRetransmissions { get; set; } = 14;

        /// <summary>
        /// Transaction timeout factor.
        /// Default: 16.</summary>
        public int TransactionTimeoutFactor { get; set; } = 16;
    }
}
