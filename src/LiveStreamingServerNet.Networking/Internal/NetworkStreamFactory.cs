using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class NetworkStreamFactory : INetworkStreamFactory
    {
        private readonly SecurityConfiguration _config;

        public NetworkStreamFactory(IOptions<SecurityConfiguration> config)
        {
            _config = config.Value;
        }

        public async Task<Stream> CreateNetworkStreamAsync(
            ITcpClientInternal tcpClient,
            ServerEndPoint serverEndPoint,
            CancellationToken cancellationToken)
        {
            if (serverEndPoint.IsSecure && _config.ServerCertificate != null)
            {
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

            return tcpClient.GetStream();
        }
    }
}
