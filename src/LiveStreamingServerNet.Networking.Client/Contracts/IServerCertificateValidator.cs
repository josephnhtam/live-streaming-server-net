using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LiveStreamingServerNet.Networking.Client.Contracts
{
    public interface IServerCertificateValidator
    {
        bool Validate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors);
    }
}
