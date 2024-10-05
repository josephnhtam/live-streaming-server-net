using LiveStreamingServerNet.StreamProcessor.Internal.Utilities;

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
                var spsLen = data.Slice(pos).ReadInt16BigEndian();
                sps.Add(data.Slice(pos + 2, spsLen));
                pos += 2 + spsLen;
            }

            var ppsCount = data[pos++];

            if (ppsCount == 0)
                throw new InvalidOperationException("PPS count is 0");

            for (int i = 0; i < ppsCount; i++)
            {
                var ppsLen = data.Slice(pos).ReadInt16BigEndian();
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

        public static HEVCSequenceHeader ParseHEVCSequenceHeader(ArraySegment<byte> data)
        {
            if (data.Count < 23)
                throw new ArgumentException("Invalid HEVC sequence header");

            var vps = new List<ArraySegment<byte>>();
            var sps = new List<ArraySegment<byte>>();
            var pps = new List<ArraySegment<byte>>();

            var pos = 0;

            var configVersion = data[pos++];
            var generalProfileSpace = (byte)(data[pos] >> 6);
            var generalTierFlag = (byte)((data[pos] >> 5) & 0x01);
            var generalProfileIdc = (byte)(data[pos++] & 0x1f);
            var generalProfileCompatibilityFlags = (uint)data.Slice(pos).ReadInt32BigEndian(); pos += 4;
            var generalConstraintIndicatorFlags = (uint)data.Slice(pos).ReadInt48BigEndian(); pos += 6;
            var generalLevelIdc = data[pos++];
            var minSpatialSegmentationIdc = (uint)(data.Slice(pos).ReadInt16BigEndian() & 0x0fff); pos += 2;
            var parallelismType = (byte)(data[pos++] & 0x03);
            var chromaFormat = (byte)(data[pos++] >> 5);
            var bitDepthLumaMinus8 = (byte)(data[pos++] & 0x07);
            var bitDepthChromaMinus8 = (byte)(data[pos++] & 0x07);
            var avgFrameRate = (ushort)data.Slice(pos).ReadInt16BigEndian(); pos += 2;
            var constantFrameRate = (byte)(data[pos] >> 6);
            var numTemporalLayers = (byte)((data[pos] >> 3) & 0x07);
            var temporalIdNested = (byte)((data[pos] >> 2) & 0x01);
            var naluLengthSizeMinusOne = (byte)(data[pos++] & 0x03);

            var numOfArrays = data[pos++];

            for (int i = 0; i < numOfArrays; i++)
            {
                var naluType = (NALUType)(data[pos++] & 0x3f);
                var numNalus = (ushort)data.Slice(pos).ReadInt16BigEndian(); pos += 2;

                for (int j = 0; j < numNalus; j++)
                {
                    var naluLen = data.Slice(pos).ReadInt16BigEndian();
                    var nalu = data.Slice(pos + 2, naluLen);
                    pos += 2 + naluLen;

                    switch (naluType)
                    {
                        case NALUType.VPS:
                            vps.Add(nalu);
                            break;
                        case NALUType.SPS:
                            sps.Add(nalu);
                            break;
                        case NALUType.PPS:
                            pps.Add(nalu);
                            break;
                    }
                }
            }

            if (vps.Count == 0)
                throw new InvalidOperationException("VPS count is 0");

            if (sps.Count == 0)
                throw new InvalidOperationException("SPS count is 0");

            if (pps.Count == 0)
                throw new InvalidOperationException("PPS count is 0");

            return new HEVCSequenceHeader(
                configVersion,
                generalProfileSpace,
                generalTierFlag,
                generalProfileIdc,
                generalProfileCompatibilityFlags,
                generalConstraintIndicatorFlags,
                generalLevelIdc,
                minSpatialSegmentationIdc,
                parallelismType,
                chromaFormat,
                bitDepthLumaMinus8,
                bitDepthChromaMinus8,
                avgFrameRate,
                constantFrameRate,
                numTemporalLayers,
                temporalIdNested,
                naluLengthSizeMinusOne,
                vps.First().ToArray(),
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
                var naluSize = data.ReadInt32BigEndian();

                if (naluSize > data.Count - 4)
                    return null;

                nalus.Add(data.Slice(4, naluSize));
                data = data.Slice(4 + naluSize);
            }

            return nalus;
        }

        private static List<ArraySegment<byte>>? SplitAnnexB(ArraySegment<byte> data)
        {
            var startCodeA = data.ReadInt24BigEndian();
            var startCodeB = data.ReadInt32BigEndian();

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

                    startCodeA = data.Slice(pos).ReadInt24BigEndian();

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
    }
}
