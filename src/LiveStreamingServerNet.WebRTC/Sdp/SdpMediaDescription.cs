using LiveStreamingServerNet.Utilities.Extensions;
using LiveStreamingServerNet.WebRTC.Sdp.Contracts;
using System.Text;

namespace LiveStreamingServerNet.WebRTC.Sdp
{
    public sealed record SdpMediaDescription
    {
        private const string CrLf = "\r\n";

        public required string Media { get; init; }
        public required int Port { get; init; }
        public required string Proto { get; init; }
        public IReadOnlyList<string> Formats { get; init; } = [];
        public SdpConnection? Connection { get; init; }
        public IReadOnlySdpAttributes Attributes { get; init; } = new SdpAttributes();
        public IReadOnlyList<string> OtherLines { get; init; } = [];

        public string? Mid => Attributes.GetAttributeValue(SdpAttributeNames.Mid);
        public string? IceUfrag => Attributes.GetAttributeValue(SdpAttributeNames.IceUfrag);
        public string? IcePwd => Attributes.GetAttributeValue(SdpAttributeNames.IcePwd);

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("m=")
                .Append(Media)
                .Append(' ')
                .Append(Port)
                .Append(' ')
                .Append(Proto);

            foreach (var fmt in Formats)
                sb.Append(' ').Append(fmt);

            sb.Append(CrLf);

            if (Connection.HasValue)
                sb.Append(Connection.Value).Append(CrLf);

            foreach (var line in OtherLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    sb.Append(line.TrimEnd('\r', '\n')).Append(CrLf);
            }

            foreach (var a in Attributes)
            {
                var line = a.ToString();
                if (!string.IsNullOrWhiteSpace(line))
                    sb.Append(line).Append(CrLf);
            }

            return sb.ToString();
        }

        public static SdpMediaDescription ParseValue(string value)
        {
            var parts = value.SplitBySpaces();

            if (parts.Length < 3)
                throw new FormatException("Invalid m= line");

            var portToken = parts[1];
            var slash = portToken.IndexOf('/');
            if (slash >= 0)
                portToken = portToken[..slash];

            if (!int.TryParse(portToken, out var port))
                throw new FormatException("Invalid m= port");

            var formats = new List<string>();
            for (var i = 3; i < parts.Length; i++)
                formats.Add(parts[i]);

            return new SdpMediaDescription
            {
                Media = parts[0],
                Port = port,
                Proto = parts[2],
                Formats = formats
            };
        }
    }
}
