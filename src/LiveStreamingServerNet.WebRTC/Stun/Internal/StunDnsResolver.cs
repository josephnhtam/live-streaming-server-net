using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal
{
    internal partial class StunDnsResolver : IStunDnsResolver
    {
        private readonly int _defaultPort;

        [GeneratedRegex(@"^(?:(?<scheme>stuns?):)?(?<host>\[[:0-9a-fA-F]+\]|[^:\s]+)(?::(?<port>\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex StunUriRegex();

        public StunDnsResolver(int defaultPort = 3478)
        {
            _defaultPort = defaultPort;
        }

        public async Task<IPEndPoint[]> ResolveAsync(string stunUri, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(stunUri))
            {
                throw new ArgumentException(nameof(stunUri));
            }

            var match = StunUriRegex().Match(stunUri);
            if (!match.Success)
            {
                throw new ArgumentException($"The STUN URI '{stunUri}' is not valid.");
            }

            var hostGroup = match.Groups["host"];
            var host = hostGroup.Value.Trim('[', ']');

            var portGroup = match.Groups["port"];
            var port = portGroup.Success && int.TryParse(portGroup.Value, out var p) ? p : _defaultPort;

            if (IPAddress.TryParse(host, out var ipAddress))
            {
                return [new IPEndPoint(ipAddress, port)];
            }

            try
            {
                var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false);
                return addresses.Select(address => new IPEndPoint(address, port)).ToArray();
            }
            catch (SocketException)
            {
                return [];
            }
        }
    }
}
