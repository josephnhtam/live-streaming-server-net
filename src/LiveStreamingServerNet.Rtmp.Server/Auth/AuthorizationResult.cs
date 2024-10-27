namespace LiveStreamingServerNet.Rtmp.Server.Auth
{
    /// <summary>
    /// Represents the result of an RTMP stream authorization request.
    /// </summary>
    public class AuthorizationResult
    {
        /// <summary>
        /// Gets or sets whether the request is authorized.
        /// </summary>
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// Gets or sets the reason for denial when not authorized.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Gets or sets an alternative stream path to use instead of the requested path.
        /// </summary>
        public string? StreamPathOverride { get; set; }

        /// <summary>
        /// Gets or sets alternative stream arguments to use instead of the provided arguments.
        /// </summary>
        public IReadOnlyDictionary<string, string>? StreamArgumentsOverride { get; set; }

        private AuthorizationResult() { }

        /// <summary>
        /// Creates a successful authorization result with optional path and arguments overrides.
        /// </summary>
        /// <param name="streamPathOverride">Alternative stream path to use</param>
        /// <param name="streamArgumentsOverride">Alternative stream arguments to use</param>
        /// <returns>An authorized result</returns>
        public static AuthorizationResult Authorized(string? streamPathOverride = null, IReadOnlyDictionary<string, string>? streamArgumentsOverride = null)
        {
            return new AuthorizationResult
            {
                IsAuthorized = true,
                StreamPathOverride = streamPathOverride,
                StreamArgumentsOverride = streamArgumentsOverride
            };
        }

        /// <summary>
        /// Creates a failed authorization result with a denial reason.
        /// </summary>
        /// <param name="reason">The reason for denying authorization</param>
        /// <returns>An unauthorized result</returns>
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
