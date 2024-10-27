using System.Net.Security;

namespace LiveStreamingServerNet.Networking.Client.Configurations
{
    /// <summary>
    /// Configuration class for SSL/TLS security settings.
    /// </summary>
    public class SecurityConfiguration
    {
        /// <summary>
        /// Gets or sets SSL client authentication options.
        /// </summary>
        public SslClientAuthenticationOptions? AuthenticationOptions { get; set; }
    }
}
