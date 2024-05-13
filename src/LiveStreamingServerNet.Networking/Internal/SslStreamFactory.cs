using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class SslStreamFactory : ISslStreamFactory
    {
        private readonly SecurityConfiguration _config;

        public SslStreamFactory(IOptions<SecurityConfiguration> config)
        {
            _config = config.Value;
        }

        public async Task<SslStream?> CreateAsync(ITcpClientInternal tcpClient, CancellationToken cancellationToken)
        {
            if (_config.ServerCertificate == null)
                return null;

            var sslStream = new SslStream(tcpClient.GetStream(), false);

            var options = new SslServerAuthenticationOptions
            {
                ServerCertificate = _config.ServerCertificate,
                ClientCertificateRequired = false,
                EnabledSslProtocols = _config.SslProtocols,
                CertificateRevocationCheckMode = _config.CheckCertificateRevocation ?
                        X509RevocationMode.Online : X509RevocationMode.NoCheck,
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
            };

            await sslStream.AuthenticateAsServerAsync(options, cancellationToken);

            return sslStream;
        }
    }
}
