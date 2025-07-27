using LiveStreamingServerNet.Networking.Client.Configurations;
using LiveStreamingServerNet.Networking.Client.Contracts;
using LiveStreamingServerNet.Networking.Client.Internal.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LiveStreamingServerNet.Networking.Client.Internal
{
    internal class SslStreamFactory : ISslStreamFactory
    {
        private readonly SecurityConfiguration _config;
        private readonly IServerCertificateValidator? _certValidator;

        public SslStreamFactory(IOptions<SecurityConfiguration> config, IServerCertificateValidator? certValidator = null)
        {
            _config = config.Value;
            _certValidator = certValidator;
        }

        public async Task<SslStream> CreateAsync(ITcpClientInternal tcpClient, CancellationToken cancellationToken)
        {
            var sslStream = new SslStream(tcpClient.GetStream(), false, ValidateServerCertificate);
            await sslStream.AuthenticateAsClientAsync(_config.AuthenticationOptions ?? new(), cancellationToken).ConfigureAwait(false);

            return sslStream;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (_certValidator != null)
                return _certValidator.Validate(sender, certificate, chain, sslPolicyErrors);

            return true;
        }
    }
}
