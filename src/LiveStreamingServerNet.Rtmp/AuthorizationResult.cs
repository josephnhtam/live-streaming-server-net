namespace LiveStreamingServerNet.Rtmp
{
    public class AuthorizationResult
    {
        public bool IsAuthorized { get; set; }
        public string Reason { get; set; } = default!;
        public string? StreamPathOverride { get; set; }
        public IDictionary<string, string>? StreamArgumentsOverride { get; set; }

        private AuthorizationResult() { }

        public static AuthorizationResult Authorized(string? StreamPathOverride = null, IDictionary<string, string>? StreamArgumentsOverride = null)
        {
            return new AuthorizationResult
            {
                IsAuthorized = true,
                StreamPathOverride = StreamPathOverride,
                StreamArgumentsOverride = StreamArgumentsOverride
            };
        }

        public static AuthorizationResult Unauthorized(string Reason)
        {
            return new AuthorizationResult
            {
                IsAuthorized = false,
                Reason = Reason
            };
        }
    }
}
