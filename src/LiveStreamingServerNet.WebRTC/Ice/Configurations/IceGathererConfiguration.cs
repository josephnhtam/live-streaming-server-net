using LiveStreamingServerNet.WebRTC.Ice.Contracts;

namespace LiveStreamingServerNet.WebRTC.Ice.Configurations
{
    public class IceGathererConfiguration
    {
        public int StunDnsResolutionMaxConcurrency { get; set; } = 4;
        public int StunBindingMaxConcurrency { get; set; } = 8;

        public int StunBindingMaxRetries { get; set; } = 3;
        public TimeSpan StunBindingRetryBaseDelay { get; set; } = TimeSpan.FromMilliseconds(200);
        public TimeSpan StunBindingAttemptTimeout { get; set; } = TimeSpan.FromSeconds(3);

        public bool GatherLoopbackCandidates { get; set; } = true;
        public ILocalPreferenceScoreProvider? LocalPreferenceScoreProvider { get; set; }

        public List<string> StunServers { get; set; } = new();
    }
}
