using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LiveStreamingServerNet.Networking.Client.Contracts
{
    /// <summary>
    /// Validates server SSL/TLS certificates during connection.
    /// </summary>
    public interface IServerCertificateValidator
    {
        /// <summary>
        /// Validates server certificate using custom logic.
        /// </summary>
        /// <param name="sender">Source of the validation request.</param>
        /// <param name="certificate">Server certificate to validate.</param>
        /// <param name="chain">Certificate chain used to validate the leaf certificate.</param>
        /// <param name="sslPolicyErrors">SSL policy errors encountered.</param>
        /// <returns>True if certificate is valid, false otherwise.</returns>
        bool Validate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors);
    }
}
