using LiveStreamingServerNet.Utilities.Extensions;
using LiveStreamingServerNet.WebRTC.Sdp.Attributes;
using LiveStreamingServerNet.WebRTC.Sdp.Contracts;
using LiveStreamingServerNet.WebRTC.Sdp.Internal;
using System.Text;

namespace LiveStreamingServerNet.WebRTC.Sdp
{
    public sealed record SdpSessionDescription
    {
        private const string CrLf = "\r\n";

        public required int Version { get; init; }
        public required SdpOrigin Origin { get; init; }
        public required string SessionName { get; init; }
        public required SdpTiming Timing { get; init; }
        public SdpConnection? Connection { get; init; }
        public IReadOnlySdpAttributes Attributes { get; init; } = new SdpAttributes();
        public IReadOnlyList<SdpMediaDescription> MediaDescriptions { get; init; } = [];
        public IReadOnlyList<string> OtherLines { get; init; } = [];

        public string? GetAttributeValue(string name) => Attributes.GetAttributeValue(name);

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("v=").Append(Version).Append(CrLf);
            sb.Append(Origin).Append(CrLf);
            sb.Append("s=").Append(SessionName).Append(CrLf);
            sb.Append(Timing).Append(CrLf);

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

            foreach (var m in MediaDescriptions)
                sb.Append(m);

            return sb.ToString();
        }

        public static SdpSessionDescription Parse(string sdp)
        {
            if (sdp is null)
                throw new ArgumentNullException(nameof(sdp));

            var builder = new SessionBuilder();
            MediaBuilder? currentMediaBuilder = null;

            var lines = sdp.Split('\n');

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.Length < 2 || line[1] != '=')
                    throw new FormatException($"Invalid SDP line: {line}");

                var type = line[0];
                var value = line[2..];

                if (type == 'm')
                {
                    if (currentMediaBuilder != null)
                        builder.MediaDescriptions.Add(currentMediaBuilder.Build());

                    currentMediaBuilder = MediaBuilder.ParseValue(value);
                    continue;
                }

                if (currentMediaBuilder != null)
                {
                    ParseMediaLevelLine(currentMediaBuilder, type, value, line);
                }
                else
                {
                    ParseSessionLevelLine(builder, type, value, line);
                }
            }

            if (currentMediaBuilder != null)
                builder.MediaDescriptions.Add(currentMediaBuilder.Build());

            return builder.Build();
        }

        private static void ParseSessionLevelLine(SessionBuilder builder, char type, string value, string fullLine)
        {
            switch (type)
            {
                case 'v':
                    if (!int.TryParse(value.Trim(), out var version))
                        throw new FormatException("Invalid v= value");

                    builder.Version = version;
                    return;
                case 'o':
                    builder.Origin = SdpOrigin.ParseValue(value);
                    return;
                case 's':
                    builder.SessionName = value.Trim();
                    return;
                case 't':
                    builder.Timing = SdpTiming.ParseValue(value);
                    return;
                case 'c':
                    builder.Connection = SdpConnection.ParseValue(value);
                    return;
                case 'a':
                    var (name, attrValue) = SdpAttributeBase.ParseNameValue(value);
                    builder.Attributes.Add(SdpAttributeRegistry.Parse(name, attrValue));
                    return;
                default:
                    builder.OtherLines.Add(fullLine);
                    return;
            }
        }

        private static void ParseMediaLevelLine(MediaBuilder media, char type, string value, string fullLine)
        {
            switch (type)
            {
                case 'c':
                    media.Connection = SdpConnection.ParseValue(value);
                    return;
                case 'a':
                    var (name, attrValue) = SdpAttributeBase.ParseNameValue(value);
                    media.Attributes.Add(SdpAttributeRegistry.Parse(name, attrValue));
                    return;
                default:
                    media.OtherLines.Add(fullLine);
                    return;
            }
        }

        private sealed class SessionBuilder
        {
            public int? Version { get; set; }
            public SdpOrigin? Origin { get; set; }
            public string? SessionName { get; set; }
            public SdpTiming? Timing { get; set; }
            public SdpConnection? Connection { get; set; }
            public SdpAttributes Attributes { get; } = new();
            public List<SdpMediaDescription> MediaDescriptions { get; } = new();
            public List<string> OtherLines { get; } = new();

            public SdpSessionDescription Build()
            {
                if (!Version.HasValue)
                    throw new FormatException("Missing required v= line");

                if (!Origin.HasValue)
                    throw new FormatException("Missing required o= line");

                if (SessionName is null)
                    throw new FormatException("Missing required s= line");

                if (!Timing.HasValue)
                    throw new FormatException("Missing required t= line");

                return new SdpSessionDescription
                {
                    Version = Version.Value,
                    Origin = Origin.Value,
                    SessionName = SessionName,
                    Timing = Timing.Value,
                    Connection = Connection,
                    Attributes = Attributes,
                    MediaDescriptions = MediaDescriptions,
                    OtherLines = OtherLines
                };
            }
        }

        private sealed class MediaBuilder
        {
            public required string Media { get; init; }
            public required int Port { get; init; }
            public required string Proto { get; init; }
            public required List<string> Formats { get; init; }
            public SdpConnection? Connection { get; set; }
            public SdpAttributes Attributes { get; } = new();
            public List<string> OtherLines { get; } = new();

            public SdpMediaDescription Build() => new()
            {
                Media = Media,
                Port = Port,
                Proto = Proto,
                Formats = Formats,
                Connection = Connection,
                Attributes = Attributes,
                OtherLines = OtherLines
            };

            public static MediaBuilder ParseValue(string value)
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

                return new MediaBuilder
                {
                    Media = parts[0],
                    Port = port,
                    Proto = parts[2],
                    Formats = formats
                };
            }
        }
    }
}
