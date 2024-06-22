using LiveStreamingServerNet.StreamProcessor.Internal.Containers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Utilities;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
{
    internal partial class TsMuxer : ITsMuxer
    {
        private readonly IDataBuffer _headerBuffer;
        private readonly IDataBuffer _payloadBuffer;
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

        private uint? _segmentTimestamp;
        private int _payloadSize;
        private int _flushedCount;

        public uint SequenceNumber => _sequenceNumber;
        public int BufferSize => _payloadBuffer.Size;
        public int PayloadSize => _payloadSize;
        public uint? SegmentTimestamp => _segmentTimestamp;

        public TsMuxer(string outputPath, IBufferPool? bufferPool = null)
        {
            _outputPath = outputPath;
            _headerBuffer = new DataBuffer(bufferPool, 512);
            _payloadBuffer = new DataBuffer(bufferPool, 8192);
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

        private void WritePATPacket(IDataBuffer tsBuffer)
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

        private void WritePMTPacket(IDataBuffer tsBuffer)
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

        private void WritePSIPacket(IDataBuffer tsBuffer, ushort packetID, byte tableID, IPSITable psiTable, ref byte continuityCounter)
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
            Debug.Assert(remainingSize >= 0);

            for (int i = 0; i < remainingSize; i++)
                tsBuffer.Write(TsConstants.StuffingByte);

            IncreaseContinuityCounter(ref continuityCounter);
        }

        public bool WriteVideoPacket(ArraySegment<byte> dataBuffer, uint timestamp, uint compositionTime, bool isKeyFrame)
        {
            if (_avcSequenceHeader == null)
                return false;

            if (_segmentTimestamp == null)
                _segmentTimestamp = timestamp;

            var decodingTimestamp = timestamp * AVCConstants.H264Frequency;
            var presentationTimestamp = decodingTimestamp + compositionTime * AVCConstants.H264Frequency;

            var rawNALUs = GetRawNALUs(dataBuffer, isKeyFrame);
            var nalus = ConvertToAnnexB(rawNALUs);

            var bufferPos = _payloadBuffer.Position;

            WritePESPacket(
                _payloadBuffer,
                new BytesSegments(nalus),
                isKeyFrame,
                isKeyFrame,
                TsConstants.VideoPID,
                TsConstants.VideoSID,
                decodingTimestamp,
                presentationTimestamp,
                ref _videoContinuityCounter
            );

            _payloadSize += _payloadBuffer.Position - bufferPos;

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

            if (_segmentTimestamp == null)
                _segmentTimestamp = timestamp;

            var decodingTimestamp = timestamp * AVCConstants.H264Frequency;
            var presentationTimestamp = decodingTimestamp;

            var adtsHeader = new AudioDataTransportStreamHeader(_aacSequenceHeader, buffer.Count);
            adtsHeader.FillBuffer(_adtsBuffer);

            var dataBuffer = new BytesSegments(new List<ArraySegment<byte>> { _adtsBuffer, buffer });

            var bufferPos = _payloadBuffer.Position;

            WritePESPacket(
                _payloadBuffer,
                dataBuffer,
                true,
                false,
                TsConstants.AudioPID,
                TsConstants.AudioSID,
                decodingTimestamp,
                presentationTimestamp,
                ref _audioContinuityCounter
            );

            _payloadSize += _payloadBuffer.Position - bufferPos;

            return true;
        }

        private void WritePESPacket(IDataBuffer tsBuffer, BytesSegments dataBuffer, bool writeRAI, bool writePCR, ushort packetId, byte streamId, uint decodingTimestamp, uint presentationTimestamp, ref byte continuityCounter)
        {
            var position = 0;
            var bufferSize = dataBuffer.Length;

            var pesHeader = new PacketizedElementaryStreamHeader(streamId, decodingTimestamp, presentationTimestamp, bufferSize);

            while (position < bufferSize)
            {
                var isFirst = position == 0;

                var tsHeader = new TransportStreamHeader(isFirst, packetId, true, continuityCounter);
                var adaptationField = new AdaptationField(isFirst && writeRAI, isFirst && writePCR ? decodingTimestamp : null);

                var pesHeaderSize = isFirst ? pesHeader.Size : 0;

                var dataSize = Math.Min(
                    bufferSize - position,
                    TsConstants.TsPacketSize - tsHeader.Size - adaptationField.Size - pesHeaderSize);

                var remainingSize = TsConstants.TsPacketSize - tsHeader.Size - adaptationField.Size - pesHeaderSize - dataSize;

                if (remainingSize > 0)
                {
                    adaptationField.StuffingSize = Math.Max(0, remainingSize - (adaptationField.Present ? 0 : AdaptationField.BaseSize));

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

                IncreaseContinuityCounter(ref continuityCounter);
            }
        }

        private void IncreaseContinuityCounter(ref byte continuityCounter)
        {
            continuityCounter = (byte)((continuityCounter + 1) & 0xf);
        }

        private void WriteHeaderPackets(IDataBuffer tsBuffer)
        {
            WritePATPacket(tsBuffer);
            WritePMTPacket(tsBuffer);
        }

        public async ValueTask<TsSegmentPartial?> FlushPartialAsync()
        {
            if (_payloadBuffer.Size == 0)
                return null;

            var path = GetOutputPath();
            await FlushAsyncCore(path);

            return new TsSegmentPartial(path, SequenceNumber);
        }

        public async ValueTask<TsSegment?> FlushAsync(uint timestamp)
        {
            if (_payloadBuffer.Size == 0 && _flushedCount == 0)
                return null;

            var path = GetOutputPath();
            await FlushAsyncCore(path);

            return CompleteFlushing(timestamp, path);
        }

        private async Task FlushAsyncCore(string path)
        {
            if (_payloadBuffer.Size == 0)
                return;

            if (_flushedCount == 0)
                WriteHeaderPackets(_headerBuffer);

            await FlushBuffersAsync(path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetOutputPath()
        {
            return _outputPath.Replace("{seqNum}", _sequenceNumber.ToString());
        }

        private async ValueTask FlushBuffersAsync(string path)
        {
            using var fileStream = new FileStream(path, _flushedCount == 0 ? FileMode.Create : FileMode.Append);

            if (_flushedCount == 0)
            {
                await fileStream.WriteAsync(_headerBuffer.UnderlyingBuffer.AsMemory(0, _headerBuffer.Size));
                _headerBuffer.Reset();
            }

            await fileStream.WriteAsync(_payloadBuffer.UnderlyingBuffer.AsMemory(0, _payloadBuffer.Size));
            _payloadBuffer.Reset();

            _flushedCount++;
        }

        private TsSegment CompleteFlushing(uint timestamp, string path)
        {
            Debug.Assert(_segmentTimestamp.HasValue);

            var duration = (int)(timestamp - _segmentTimestamp);
            var segment = new TsSegment(path, _sequenceNumber, duration);

            _sequenceNumber++;
            _segmentTimestamp = null;

            _flushedCount = 0;
            _payloadSize = 0;

            return segment;
        }

        public void Dispose()
        {
            _payloadBuffer.Dispose();
        }
    }
}
