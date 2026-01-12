using LiveStreamingServerNet.Utilities.Extensions;
using LiveStreamingServerNet.WebRTC.Ice;
using LiveStreamingServerNet.WebRTC.Sdp.Internal;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Sdp.Attributes
{
    [SdpAttributeName(SdpAttributeNames.Candidate)]
    public sealed record SdpIceCandidateAttribute : SdpAttributeBase
    {
        public override string Name => SdpAttributeNames.Candidate;

        public override string? Value
        {
            get
            {
                var typeString = IceCandidateTypeToString(Type);
                var s = $"{Foundation} {ComponentId} {Transport} {Priority} {Address} {Port} typ {typeString}";

                if (RelatedAddress != null)
                    s += $" raddr {RelatedAddress}";

                if (RelatedPort.HasValue)
                    s += $" rport {RelatedPort.Value}";

                if (!string.IsNullOrWhiteSpace(TcpType))
                    s += $" tcptype {TcpType}";

                if (!string.IsNullOrWhiteSpace(Ufrag))
                    s += $" ufrag {Ufrag}";

                foreach (var (k, v) in Extensions.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
                {
                    if (IsKnownKey(k))
                        continue;

                    s += $" {k} {v}";
                }

                return s;
            }
        }

        public required string Foundation { get; init; }
        public required int ComponentId { get; init; }
        public required string Transport { get; init; }
        public required uint Priority { get; init; }
        public required IPAddress Address { get; init; }
        public required int Port { get; init; }
        public required IceCandidateType Type { get; init; }

        public IPAddress? RelatedAddress { get; init; }
        public int? RelatedPort { get; init; }

        public string? TcpType { get; init; }
        public string? Ufrag { get; init; }

        public IReadOnlyDictionary<string, string> Extensions { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static SdpIceCandidateAttribute? ParseValue(string candidateAttributeValue)
        {
            if (string.IsNullOrWhiteSpace(candidateAttributeValue))
                return null;

            var parts = candidateAttributeValue.SplitBySpaces();

            if (parts.Length < 8)
                return null;

            if (!int.TryParse(parts[1], out var componentId))
                return null;

            if (!uint.TryParse(parts[3], out var priority))
                return null;

            if (!IPAddress.TryParse(parts[4], out var address))
                return null;

            if (!int.TryParse(parts[5], out var port))
                return null;

            if (!string.Equals(parts[6], "typ", StringComparison.OrdinalIgnoreCase))
                return null;

            var type = IceCandidateTypeFromString(parts[7]);

            var relAddr = (IPAddress?)null;
            int? relPort = null;
            string? tcpType = null;
            string? ufrag = null;

            var extensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 8; i + 1 < parts.Length; i += 2)
            {
                var key = parts[i];
                var value = parts[i + 1];

                if (string.Equals(key, "raddr", StringComparison.OrdinalIgnoreCase))
                {
                    if (IPAddress.TryParse(value, out var ra))
                        relAddr = ra;
                    continue;
                }

                if (string.Equals(key, "rport", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(value, out var rp))
                        relPort = rp;
                    continue;
                }

                if (string.Equals(key, "tcptype", StringComparison.OrdinalIgnoreCase))
                {
                    tcpType = value;
                    continue;
                }

                if (string.Equals(key, "ufrag", StringComparison.OrdinalIgnoreCase))
                {
                    ufrag = value;
                    continue;
                }

                extensions[key] = value;
            }

            return new SdpIceCandidateAttribute
            {
                Foundation = parts[0],
                ComponentId = componentId,
                Transport = parts[2],
                Priority = priority,
                Address = address,
                Port = port,
                Type = type,
                RelatedAddress = relAddr,
                RelatedPort = relPort,
                TcpType = tcpType,
                Ufrag = ufrag,
                Extensions = extensions
            };
        }

        public static SdpIceCandidateAttribute FromIceCandidate(IceCandidate candidate, IPEndPoint? relatedEndPoint = null, string transport = "udp")
        {
            return new SdpIceCandidateAttribute
            {
                Foundation = candidate.Foundation,
                ComponentId = candidate.ComponentId,
                Transport = transport,
                Priority = candidate.Priority,
                Address = candidate.EndPoint.Address,
                Port = candidate.EndPoint.Port,
                Type = candidate.Type,
                RelatedAddress = relatedEndPoint?.Address,
                RelatedPort = relatedEndPoint?.Port
            };
        }

        public RemoteIceCandidate ToRemoteIceCandidate()
        {
            return new RemoteIceCandidate(
                EndPoint: new IPEndPoint(Address, Port),
                Type: Type,
                Foundation: Foundation,
                LocalPreference: 65535,
                ComponentId: ComponentId);
        }

        private static string IceCandidateTypeToString(IceCandidateType type) => type switch
        {
            IceCandidateType.Host => "host",
            IceCandidateType.ServerReflexive => "srflx",
            IceCandidateType.PeerReflexive => "prflx",
            IceCandidateType.Relayed => "relay",
            _ => "host"
        };

        private static IceCandidateType IceCandidateTypeFromString(string type) => type.ToLowerInvariant() switch
        {
            "host" => IceCandidateType.Host,
            "srflx" => IceCandidateType.ServerReflexive,
            "prflx" => IceCandidateType.PeerReflexive,
            "relay" => IceCandidateType.Relayed,
            _ => IceCandidateType.Host
        };

        private static bool IsKnownKey(string key) =>
            string.Equals(key, "raddr", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(key, "rport", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(key, "tcptype", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(key, "ufrag", StringComparison.OrdinalIgnoreCase);
    }
}
