namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
{
    internal static class AVCParser
    {
        public static AVCSequenceHeader ParseSequenceHeader(ArraySegment<byte> data)
        {
            if (data.Count < 7)
                throw new IndexOutOfRangeException(nameof(data));

            var sps = new List<ArraySegment<byte>>();
            var pps = new List<ArraySegment<byte>>();

            var pos = 0;

            var configVersion = data[pos++];
            var avcProfileIndication = data[pos++];
            var profileCompatibility = data[pos++];
            var avcLevelIndication = data[pos++];
            var naluLengthSizeMinusOne = (byte)(data[pos++] & 0x03);

            var spsCount = data[pos++] & 0x1f;

            if (spsCount == 0)
                throw new InvalidOperationException("SPS count is 0");

            for (int i = 0; i < spsCount; i++)
            {
                var spsLen = ReadInt16BigEndian(data.Slice(pos));
                sps.Add(data.Slice(pos + 2, spsLen));
                pos += 2 + spsLen;
            }

            var ppsCount = data[pos++];

            if (ppsCount == 0)
                throw new InvalidOperationException("PPS count is 0");

            for (int i = 0; i < ppsCount; i++)
            {
                var ppsLen = ReadInt16BigEndian(data.Slice(pos));
                pps.Add(data.Slice(pos + 2, ppsLen));
                pos += 2 + ppsLen;
            }

            return new AVCSequenceHeader(
                configVersion,
                avcProfileIndication,
                profileCompatibility,
                avcLevelIndication,
                naluLengthSizeMinusOne,
                sps.First().ToArray(),
                pps.First().ToArray()
            );
        }

        public static List<ArraySegment<byte>> SplitNALUs(ArraySegment<byte> data)
        {
            return SplitAVCC(data) ?? SplitAnnexB(data) ?? throw new InvalidOperationException("Invalid NALU format");
        }

        private static List<ArraySegment<byte>>? SplitAVCC(ArraySegment<byte> data)
        {
            var nalus = new List<ArraySegment<byte>>();

            while (data.Count > 0)
            {
                var naluSize = ReadInt32BigEndian(data);

                if (naluSize > data.Count - 4)
                    return null;

                nalus.Add(data.Slice(4, naluSize));
                data = data.Slice(4 + naluSize);
            }

            return nalus;
        }

        private static List<ArraySegment<byte>>? SplitAnnexB(ArraySegment<byte> data)
        {
            var startCodeA = ReadInt24BigEndian(data);
            var startCodeB = ReadInt32BigEndian(data);

            if (startCodeA != 1 && startCodeB != 1)
                return null;

            var nalus = new List<ArraySegment<byte>>();

            var start = 0;
            var pos = 0;

            while (true)
            {
                if (start != pos)
                    nalus.Add(data.Slice(start, pos - start));

                pos += startCodeA == 1 ? 3 : 4;
                start = pos;

                if (start >= data.Count)
                    break;

                startCodeA = 0;

                while (pos < data.Count)
                {
                    if ((pos + 2) >= data.Count || data[pos] != 0)
                    {
                        pos++;
                        continue;
                    }

                    startCodeA = ReadInt24BigEndian(data.Slice(pos));

                    // 0x000001 - 3 bytes start code
                    if (startCodeA == 1)
                        break;

                    // 0x00000001 - 4 bytes start code
                    if (startCodeA == 0 && (pos + 3) < data.Count && data[pos + 3] == 1)
                        break;

                    pos++;
                }
            }

            return nalus;
        }

        private static int ReadInt16BigEndian(ArraySegment<byte> buffer)
        {
            return (buffer[0] << 8) | buffer[1];
        }

        private static int ReadInt24BigEndian(ArraySegment<byte> buffer)
        {
            return (buffer[0] << 16) | (buffer[1] << 8) | buffer[2];
        }

        private static int ReadInt32BigEndian(ArraySegment<byte> buffer)
        {
            return (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
        }

    }
}
