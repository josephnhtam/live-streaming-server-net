namespace LiveStreamingServerNet.KubernetesPod.StreamRegistration
{
    public class StreamRevalidationResult
    {
        public bool Successful { get; set; }
        public bool Retryable { get; set; }
        public string? Reason { get; set; }

        private StreamRevalidationResult() { }

        public static StreamRevalidationResult Success()
        {
            return new StreamRevalidationResult
            {
                Successful = true,
            };
        }

        public static StreamRevalidationResult Failure(bool retryable, string reason)
        {
            return new StreamRevalidationResult
            {
                Successful = false,
                Retryable = retryable,
                Reason = reason
            };
        }
    }
}
