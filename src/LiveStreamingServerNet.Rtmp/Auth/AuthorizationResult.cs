namespace LiveStreamingServerNet.Rtmp.Auth
{
    public class AuthorizationResult
    {
        public bool IsAuthorized { get; set; }
        public string? Reason { get; set; }
        public string? StreamPathOverride { get; set; }
        public IReadOnlyDictionary<string, string>? StreamArgumentsOverride { get; set; }

        private AuthorizationResult() { }

        public static AuthorizationResult Authorized(string? streamPathOverride = null, IReadOnlyDictionary<string, string>? streamArgumentsOverride = null)
        {
            return new AuthorizationResult
            {
                IsAuthorized = true,
                StreamPathOverride = streamPathOverride,
                StreamArgumentsOverride = streamArgumentsOverride
            };
        }

        public static AuthorizationResult Unauthorized(string reason)
        {
            return new AuthorizationResult
            {
                IsAuthorized = false,
                Reason = reason
            };
        }
    }
}
