using System.Net.Security;

namespace LiveStreamingServerNet.Networking.Client.Configurations
{
    public class SecurityConfiguration
    {
        public SslClientAuthenticationOptions? AuthenticationOptions { get; set; }
    }
}
