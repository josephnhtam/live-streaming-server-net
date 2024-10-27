using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace LiveStreamingServerNet.Networking.Server.Configurations
{
    /// <summary>
    /// Configuration options for SSL/TLS security settings.
    /// </summary>
    public class SecurityConfiguration
    {
        /// <summary>
        /// Certificate used for server authentication.
        /// If null, SSL/TLS is not enabled.
        /// </summary>
        public X509Certificate2? ServerCertificate { get; set; }

        /// <summary>
        /// Supported SSL/TLS protocol versions.
        /// Default: TLS 1.2.
        /// </summary>
        public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls12;

        /// <summary>
        /// Whether to check certificate revocation status.
        /// Default: true.
        /// </summary>
        public bool CheckCertificateRevocation { get; set; } = true;
    }
}
