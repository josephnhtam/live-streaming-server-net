using LiveStreamingServerNet.Networking;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace LiveStreamingServerNet.Rtmp.Client
{
    public static class RtmpUrlParser
    {
        private static Regex _regex = new Regex(
            @"^(?<scheme>rtmp[s]?)://(?<hostname>\[[^\]]+\]|[^:/]+)(?::(?<port>\d+))?/(?<appName>[^/]+)/(?<streamName>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public static async ValueTask<ParsedRtmpUrl> ParseAsync(string rtmpUrl)
        {
            if (string.IsNullOrWhiteSpace(rtmpUrl))
                throw new ArgumentException("RTMP URL cannot be null or empty.");

            rtmpUrl = rtmpUrl.Trim();

            var match = _regex.Match(rtmpUrl);

            if (!match.Success)
                throw new ArgumentException("Invalid RTMP URL format.");

            var schemeRaw = match.Groups["scheme"].Value.ToLower();
            var hostnameRaw = match.Groups["hostname"].Value;
            var portStr = match.Groups["port"].Value;
            var appName = match.Groups["appName"].Value;
            var streamName = match.Groups["streamName"].Value;

            if (!Enum.TryParse<RtmpScheme>(schemeRaw, true, out var scheme))
                throw new ArgumentException("Invalid RTMP scheme in RTMP URL.");

            var hostname = NormalizeHostname(hostnameRaw);

            var port = portStr switch
            {
                var str when (string.IsNullOrEmpty(str)) => scheme == RtmpScheme.RTMPS ? 443 : 1935,
                _ => int.TryParse(portStr, out var p) ? p : throw new ArgumentException("Invalid port number in RTMP URL.")
            };

            var ipAddress = await GetIpAddressAsync(hostname);
            var serverEndPoint = new ServerEndPoint(new IPEndPoint(ipAddress, port), scheme == RtmpScheme.RTMPS);
            var tcUrl = $"{schemeRaw}://{hostname}:{port}/{appName}";
            return new ParsedRtmpUrl(serverEndPoint, appName, streamName, tcUrl);
        }

        private static string NormalizeHostname(string hostnameRaw)
        {
            if (hostnameRaw.StartsWith("[") && hostnameRaw.EndsWith("]"))
                return hostnameRaw.Substring(1, hostnameRaw.Length - 2);

            return hostnameRaw;
        }

        private static async ValueTask<IPAddress> GetIpAddressAsync(string hostname)
        {
            if (IPAddress.TryParse(hostname, out var ipAddress) && ipAddress != null)
                return ipAddress;

            return await ResolveHostnameToIpAddressAsync(hostname);
        }

        private static async Task<IPAddress> ResolveHostnameToIpAddressAsync(string hostname)
        {
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(hostname);

                foreach (var address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                        return address;
                }

                foreach (var address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetworkV6)
                        return address;
                }

                throw new SocketException((int)SocketError.HostNotFound);
            }
            catch (SocketException ex)
            {
                throw new SocketException((int)ex.SocketErrorCode);
            }
        }

        private enum RtmpScheme
        {
            RTMP,
            RTMPS
        }
    }

    public record ParsedRtmpUrl(ServerEndPoint ServerEndPoint, string AppName, string StreamName, string TcUrl);
}
