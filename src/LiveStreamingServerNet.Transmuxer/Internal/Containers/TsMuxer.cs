using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Containers.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Utilities;
using System.Diagnostics;

namespace LiveStreamingServerNet.Transmuxer.Internal.Containers
{
    internal partial class TsMuxer : ITsMuxer
    {
        private readonly INetBuffer _headerBuffer;
        private readonly INetBuffer _payloadBuffer;
        private readonly byte[] _adtsBuffer;
        private readonly string _outputPath;

        private AVCSequenceHeader? _avcSequenceHeader;
        private AACSequenceHeader? _aacSequenceHeader;

        private byte _patContinuityCounter;
        private byte _pmtContinuityCounter;
        private byte _videoContinuityCounter;
        private byte _audioContinuityCounter;
        private uint _sequenceNumber;
        private byte[]? _patBuffer;

        private uint? _timestampStart;

        public uint SequenceNumber => _sequenceNumber;
        public int BufferSize => _payloadBuffer.Size;

        public TsMuxer(string outputPath)
        {
            _outputPath = outputPath;
            _headerBuffer = new NetBuffer(512);
            _payloadBuffer = new NetBuffer(8192);
            _adtsBuffer = new byte[AudioDataTransportStreamHeader.Size];
        }

        public void SetAVCSequenceHeader(AVCSequenceHeader avcSequenceHeader)
        {
            _avcSequenceHeader = avcSequenceHeader;
        }

        public void SetAACSequenceHeader(AACSequenceHeader aacSequenceHeader)
        {
            _aacSequenceHeader = aacSequenceHeader;
        }

        private void WritePATPacket(INetBuffer tsBuffer)
        {
            var pat = new ProgramAssociationTable(
                TsConstants.TransportStreamIdentifier,
                TsConstants.ProgramNumber,
                TsConstants.ProgramMapPID
            );

            WritePSIPacket(
                tsBuffer,
                TsConstants.ProgramAssociationPID,
                TsConstants.ProgramAssociationTableID,
                pat,
                ref _patContinuityCounter
            );
        }

        private void WritePMTPacket(INetBuffer tsBuffer)
        {
            var elementaryStreamInfos = new List<ElementaryStreamInfo>();

            if (_avcSequenceHeader != null)
                elementaryStreamInfos.Add(new ElementaryStreamInfo(TsConstants.AVCStreamType, TsConstants.VideoPID));

            if (_aacSequenceHeader != null)
                elementaryStreamInfos.Add(new ElementaryStreamInfo(TsConstants.AACStreamType, TsConstants.AudioPID));

            var pmt = new ProgramMapTable(
                TsConstants.ProgramNumber,
                TsConstants.VideoPID,
                elementaryStreamInfos
            );

            WritePSIPacket(
                tsBuffer,
                TsConstants.ProgramMapPID,
                TsConstants.ProgramMapTableID,
                pmt,
                ref _pmtContinuityCounter
            );
        }

        private void WritePSIPacket(INetBuffer tsBuffer, ushort packetID, byte tableID, IPSITable psiTable, ref byte continuityCounter)
        {
            var startPosition = tsBuffer.Position;

            var tsHeader = new TransportStreamHeader(true, packetID, true, continuityCounter);
            tsHeader.Write(tsBuffer);

            var psiStartPosition = tsBuffer.Position;

            var psiHeader = new ProgramSpecificInformationHeader(tableID, psiTable.Size);
            psiHeader.Write(tsBuffer);
            psiTable.Write(tsBuffer);

            var checksumStart = psiStartPosition + psiHeader.ChecksumOffset;
            var checksumLength = tsBuffer.Position - checksumStart;
            var checksum = new TableChecksum(tsBuffer.UnderlyingBuffer, checksumStart, checksumLength);
            checksum.Write(tsBuffer);

            var remainingSize = TsConstants.TsPacketSize - (tsBuffer.Position - startPosition);
            for (int i = 0; i < remainingSize; i++)
                tsBuffer.Write(TsConstants.StuffingByte);

            IncreaseContinuityCounter(ref continuityCounter);
        }

        public bool WriteVideoPacket(ArraySegment<byte> dataBuffer, uint timestamp, uint compositionTime, bool isKeyFrame)
        {
            if (_avcSequenceHeader == null)
                return false;

            if (_timestampStart == null)
                _timestampStart = timestamp;

            var decodingTimestamp = (int)(timestamp * AVCConstants.H264Frequency);
            var presentationTimestamp = decodingTimestamp + (int)(compositionTime * AVCConstants.H264Frequency);

            var rawNALUs = GetRawNALUs(dataBuffer, isKeyFrame);
            var nalus = ConvertToAnnexB(rawNALUs);

            WritePESPacket(
                _payloadBuffer,
                new BytesSegments(nalus),
                isKeyFrame,
                false,
                true,
                TsConstants.VideoPID,
                TsConstants.VideoSID,
                decodingTimestamp,
                presentationTimestamp,
                _videoContinuityCounter
            );

            IncreaseContinuityCounter(ref _videoContinuityCounter);
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

            rawNALUs.AddRange(AVCParser.SplitNALUs(dataBuffer));
            return rawNALUs;
        }

        private List<ArraySegment<byte>> ConvertToAnnexB(List<ArraySegment<byte>> rawNALUs)
        {
            Debug.Assert(_avcSequenceHeader != null);

            var nalus = new List<ArraySegment<byte>>(1 + rawNALUs.Count * 2) { AVCConstants.NALU_AUD };

            for (int i = 0; i < rawNALUs.Count; i++)
            {
                nalus.Add(AVCConstants.NALU_StartCode);
                nalus.Add(rawNALUs[i]);
            }

            return nalus;
        }

        public bool WriteAudioPacket(ArraySegment<byte> buffer, uint timestamp)
        {
            if (_aacSequenceHeader == null)
                return false;

            if (_timestampStart == null)
                _timestampStart = timestamp;

            var decodingTimestamp = (int)(timestamp * AVCConstants.H264Frequency);
            var presentationTimestamp = decodingTimestamp;

            var adtsHeader = new AudioDataTransportStreamHeader(_aacSequenceHeader, buffer.Count);
            adtsHeader.FillBuffer(_adtsBuffer);

            var dataBuffer = new BytesSegments(new List<ArraySegment<byte>> { _adtsBuffer, buffer });

            WritePESPacket(
                _payloadBuffer,
                dataBuffer,
                true,
                true,
                false,
                TsConstants.AudioPID,
                TsConstants.AudioSID,
                decodingTimestamp,
                presentationTimestamp,
                _audioContinuityCounter
            );

            IncreaseContinuityCounter(ref _audioContinuityCounter);
            return true;
        }

        private void WritePESPacket(INetBuffer tsBuffer, BytesSegments dataBuffer, bool isKeyFrame, bool writeRAI, bool writePCR, ushort packetId, byte streamId, int decodingTimestamp, int presentationTimestamp, byte continuityCounter)
        {
            var position = 0;
            var bufferSize = dataBuffer.Length;

            var pesHeader = new PacketizedElementaryStreamHeader(streamId, decodingTimestamp, presentationTimestamp, bufferSize);

            while (position < bufferSize)
            {
                var isFirst = position == 0;
                var isKeyFrameInit = isFirst && isKeyFrame;

                var tsHeader = new TransportStreamHeader(isFirst, packetId, true, continuityCounter);
                var adaptationField = new AdaptationField(isKeyFrameInit && writeRAI, isKeyFrameInit && writePCR ? decodingTimestamp : null);

                var pesHeaderSize = isFirst ? pesHeader.Size : 0;

                var dataSize = Math.Min(
                    bufferSize - position,
                    TsConstants.TsPacketSize - tsHeader.Size - adaptationField.Size - pesHeaderSize);

                var remainingSize = TsConstants.TsPacketSize - tsHeader.Size - adaptationField.Size - pesHeaderSize - dataSize;

                if (remainingSize > 0)
                {
                    adaptationField.StuffingSize = remainingSize - (adaptationField.Present ? 0 : AdaptationField.BaseSize);

                    dataSize = Math.Min(
                        bufferSize - position,
                        TsConstants.TsPacketSize - tsHeader.Size - adaptationField.Size - pesHeaderSize);

                    Debug.Assert((tsHeader.Size + adaptationField.Size + pesHeaderSize + dataSize) == TsConstants.TsPacketSize);
                }

                tsHeader.HasAdaptionField = adaptationField.Present;

                tsHeader.Write(tsBuffer);
                adaptationField.Write(tsBuffer);

                if (isFirst) pesHeader.Write(tsBuffer);
                dataBuffer.WriteTo(tsBuffer, position, dataSize);

                position += dataSize;
            }
        }

        private void IncreaseContinuityCounter(ref byte continuityCounter)
        {
            continuityCounter = (byte)((continuityCounter + 1) & 0xf);
        }

        private void WriteHeaderPackets(INetBuffer tsBuffer)
        {
            WritePATPacket(tsBuffer);
            WritePMTPacket(tsBuffer);
        }

        public async ValueTask<TsSegment?> FlushAsync(uint timestamp)
        {
            if (_payloadBuffer.Size > 0)
            {
                Debug.Assert(_timestampStart.HasValue);
                var duration = (int)(timestamp - _timestampStart);

                WriteHeaderPackets(_headerBuffer);

                var path = _outputPath.Replace("{seqNum}", _sequenceNumber.ToString());
                using var fileStream = new FileStream(path, FileMode.OpenOrCreate);
                await fileStream.WriteAsync(_headerBuffer.UnderlyingBuffer.AsMemory(0, _headerBuffer.Size));
                await fileStream.WriteAsync(_payloadBuffer.UnderlyingBuffer.AsMemory(0, _payloadBuffer.Size));

                var segment = new TsSegment(path, _sequenceNumber, duration);

                _headerBuffer.Reset();
                _payloadBuffer.Reset();

                _sequenceNumber++;
                _timestampStart = null;

                return segment;
            }

            return null;
        }

        public void Dispose()
        {
            _payloadBuffer.Dispose();
        }
    }
}
