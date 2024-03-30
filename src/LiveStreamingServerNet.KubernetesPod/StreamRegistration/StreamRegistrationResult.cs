namespace LiveStreamingServerNet.KubernetesPod.StreamRegistration
{
    public class StreamRegistrationResult
    {
        public bool Successful { get; set; }
        public string? Reason { get; set; }

        private StreamRegistrationResult() { }

        public static StreamRegistrationResult Success()
        {
            return new StreamRegistrationResult
            {
                Successful = true
            };
        }

        public static StreamRegistrationResult Failure(string reason)
        {
            return new StreamRegistrationResult
            {
                Successful = false,
                Reason = reason
            };
        }
    }
}
