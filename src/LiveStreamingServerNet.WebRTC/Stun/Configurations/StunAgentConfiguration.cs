namespace LiveStreamingServerNet.WebRTC.Stun.Configurations
{
    public class StunAgentConfiguration
    {
        /// <summary>
        /// Automatically handle unknown comprehension-required attributes by returning an 420 error response.
        /// Default: true.
        /// </summary>
        public bool HandleUnknownComprehensionRequiredAttributes { get; set; } = true;

        /// <summary>
        /// Default retransmission options for STUN transactions.
        /// </summary>
        public StunRetransmissionOptions RetransmissionOptions { get; set; } = new StunRetransmissionOptions();
    }
}
