using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Utilities;
using System.Diagnostics;

namespace LiveStreamingServerNet.Transmuxer.Internal.Containers
{
    internal partial class TsMuxer : IDisposable
    {
        private readonly INetBuffer _tsBuffer;
        private readonly byte[] _adtsBuffer;
        private readonly string _outputPath;

        private AvcSequenceHeader? _avcSequenceHeader;
        private AacSequenceHeader? _aacSequenceHeader;

        private byte _videoContinuityCounter;
        private byte _audioContinuityCounter;
        private uint _sequenceNumber;

        public TsMuxer(string outputPath)
        {
            _outputPath = outputPath;
            _tsBuffer = new NetBuffer(8192);
            _adtsBuffer = new byte[AudioDataTransportStreamHeader.Size];
        }

        public void SetAvcSequenceHeader(AvcSequenceHeader avcSequenceHeader)
        {
            _avcSequenceHeader = avcSequenceHeader;
        }

        public void SetAacSequenceHeader(AacSequenceHeader aacSequenceHeader)
        {
            _aacSequenceHeader = aacSequenceHeader;
        }

        public bool WriteVideoPacket(ArraySegment<byte> dataBuffer, uint timestamp, uint compositionTime, bool isKeyFrame)
        {
            if (_avcSequenceHeader == null)
                return false;

            var decodingTimestamp = (int)(timestamp * TsConstants.H264Frequency);
            var presentationTimestamp = decodingTimestamp + (int)(compositionTime * TsConstants.H264Frequency);

            var rawNALUs = GetRawNALUs(dataBuffer, isKeyFrame);
            var nalus = ConvertToAnnexB(rawNALUs);

            WritePacket(
                _tsBuffer,
                new BytesSegments(nalus),
                isKeyFrame,
                TsConstants.VideoPID,
                TsConstants.VideoSID,
                decodingTimestamp,
                presentationTimestamp,
                ref _videoContinuityCounter
            );

            return true;
        }

        private List<ArraySegment<byte>> GetRawNALUs(ArraySegment<byte> dataBuffer, bool isKeyFrame)
        {
            Debug.Assert(_avcSequenceHeader != null);

            var rawNALUs = new List<ArraySegment<byte>>();

            if (isKeyFrame)
            {
                rawNALUs.Add(_avcSequenceHeader.SPS);
                rawNALUs.Add(_avcSequenceHeader.PPS);
            }

            rawNALUs.AddRange(AvcParser.SplitNALUs(dataBuffer));

            return rawNALUs;
        }

        private List<ArraySegment<byte>> ConvertToAnnexB(List<ArraySegment<byte>> rawNALUs)
        {
            Debug.Assert(_avcSequenceHeader != null);

            var nalus = new List<ArraySegment<byte>>(1 + rawNALUs.Count * 2) { AvcConstants.NALU_AUD };

            for (int i = 0; i < rawNALUs.Count; i++)
            {
                nalus.Add(AvcConstants.NALU_StartCode);
                nalus.Add(rawNALUs[i]);
            }

            return nalus;
        }

        public bool WriteAudioPacket(ArraySegment<byte> buffer, uint timestamp)
        {
            if (_aacSequenceHeader == null)
                return false;

            var decodingTimestamp = (int)(timestamp * TsConstants.H264Frequency);
            var presentationTimestamp = decodingTimestamp;

            var adtsHeader = new AudioDataTransportStreamHeader(_aacSequenceHeader, buffer.Count);
            adtsHeader.FillBuffer(_adtsBuffer);

            var dataBuffer = new BytesSegments(new List<ArraySegment<byte>> { _adtsBuffer, buffer });

            WritePacket(
               _tsBuffer,
               dataBuffer,
               true,
               TsConstants.AudioPID,
               TsConstants.AudioSID,
               decodingTimestamp,
               presentationTimestamp,
               ref _audioContinuityCounter
           );

            return true;
        }

        private void WritePacket(INetBuffer tsBuffer, BytesSegments dataBuffer, bool isKeyFrame, short packetId, byte streamId, int decodingTimestamp, int presentationTimestamp, ref byte continuityCounter)
        {
            var position = 0;
            var bufferSize = dataBuffer.Length;

            var pesHeader = new PacketizedElementaryStreamHeader(streamId, decodingTimestamp, presentationTimestamp, bufferSize - position);

            while (position < bufferSize)
            {
                var isFirst = position == 0;

                var tsHeader = new TransportStreamHeader(isFirst, packetId, true, continuityCounter);
                var adaptionField = new AdaptionField((isFirst && isKeyFrame) ? decodingTimestamp : null);

                var pesHeaderSize = isFirst ? pesHeader.Size : 0;

                var dataSize = Math.Min(
                    bufferSize - position,
                    TsConstants.TsPacketSize - tsHeader.Size - adaptionField.Size - pesHeaderSize);

                var remainingSize = TsConstants.TsPacketSize - tsHeader.Size - adaptionField.Size - pesHeaderSize - dataSize;

                if (remainingSize > 0)
                {
                    adaptionField.StuffingSize = remainingSize - (adaptionField.Present ? 0 : AdaptionField.BaseSize);

                    dataSize = Math.Min(
                        bufferSize - position,
                        TsConstants.TsPacketSize - tsHeader.Size - adaptionField.Size - pesHeaderSize);

                    Debug.Assert((tsHeader.Size + adaptionField.Size + pesHeaderSize + dataSize) == TsConstants.TsPacketSize);
                }

                tsHeader.HasAdaptionField = adaptionField.Present;

                tsHeader.Write(tsBuffer);
                adaptionField.Write(tsBuffer);

                if (isFirst) pesHeader.Write(tsBuffer);
                dataBuffer.WriteTo(tsBuffer, position, dataSize);

                IncreaseContinuityCounter(ref continuityCounter);
                position += dataSize;
            }
        }

        private void IncreaseContinuityCounter(ref byte continuityCounter)
        {
            continuityCounter = (byte)((continuityCounter + 1) & 0xf);
        }

        public async ValueTask<string?> FlushAsync()
        {
            if (_tsBuffer.Size > 0)
            {
                var path = _outputPath.Replace("{seqNum}", _sequenceNumber.ToString());

                using var fileStream = new FileStream(path, FileMode.OpenOrCreate);

                await fileStream.WriteAsync(TsConstants.TsHeader);
                await fileStream.WriteAsync(_tsBuffer.UnderlyingBuffer.AsMemory(0, _tsBuffer.Size));

                _tsBuffer.Reset();
                _sequenceNumber++;

                return path;
            }

            return null;
        }

        public void Dispose()
        {
            _tsBuffer.Dispose();
        }
    }
}
