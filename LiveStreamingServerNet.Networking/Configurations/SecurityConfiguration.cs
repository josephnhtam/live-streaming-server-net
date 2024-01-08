using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace LiveStreamingServerNet.Networking.Configurations
{
    public class SecurityConfiguration
    {
        public X509Certificate2? ServerCertificate { get; set; }
        public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls12;
        public bool CheckCertificateRevocation { get; set; } = true;
    }
}
