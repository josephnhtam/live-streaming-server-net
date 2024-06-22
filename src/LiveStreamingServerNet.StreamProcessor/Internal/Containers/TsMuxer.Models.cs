using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers
{
    internal record struct TsSegmentPartial(string FilePath, uint SequenceNumber);
    internal record struct TsSegment(string FilePath, uint SequenceNumber, int Duration);

    internal partial class TsMuxer
    {
        private record struct TransportStreamHeader(
            bool IsFirst, ushort PacketId, bool HasPayload, byte ContinuityCounter)
        {
            public int Size => 4;
            public bool HasAdaptionField { get; set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(IDataBuffer dataBuffer)
            {
                WriteSyncByte(dataBuffer);
                WriteUnitStartIndicatorAndPacketId(dataBuffer);
                WriteContinuityCounterAndAdaptationFieldControl(dataBuffer);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void WriteSyncByte(IDataBuffer dataBuffer)
            {
                dataBuffer.Write(TsConstants.SyncByte);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteUnitStartIndicatorAndPacketId(IDataBuffer dataBuffer)
            {
                var firstByte = (byte)(PacketId >> 8);
                var secondByte = (byte)(PacketId & 0xff);

                if (IsFirst) firstByte |= 0x40;

                dataBuffer.Write(firstByte);
                dataBuffer.Write(secondByte);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteContinuityCounterAndAdaptationFieldControl(
                IDataBuffer dataBuffer)
            {
                var @byte = (byte)(ContinuityCounter & 0xf);
                if (HasPayload) @byte |= 0x10;
                if (HasAdaptionField) @byte |= 0x20;

                dataBuffer.Write(@byte);
            }
        }

        public record struct AdaptationField(bool AllowRandomAccess, uint? DecodingTimestamp)
        {
            public const int BaseSize = 2;
            public int Size => (Present ? BaseSize : 0) + (DecodingTimestamp.HasValue ? 6 : 0) + (StuffingSize ?? 0);
            public int? StuffingSize { get; set; }
            public bool Present => AllowRandomAccess || DecodingTimestamp.HasValue || StuffingSize.HasValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(IDataBuffer dataBuffer)
            {
                if (!Present) return;

                WriteSizeAndFlags(dataBuffer);
                WritePCR(dataBuffer);
                WriteStuffing(dataBuffer);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteSizeAndFlags(IDataBuffer dataBuffer)
            {
                var size = (byte)(Size - 1);

                byte flags = 0x00;
                if (DecodingTimestamp.HasValue) flags |= 0x10;
                if (AllowRandomAccess) flags |= 0x40;

                dataBuffer.Write(size);
                dataBuffer.Write(flags);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WritePCR(IDataBuffer dataBuffer)
            {
                if (!DecodingTimestamp.HasValue) return;

                dataBuffer.Write((byte)(DecodingTimestamp.Value >> 25));
                dataBuffer.Write((byte)(DecodingTimestamp.Value >> 17));
                dataBuffer.Write((byte)(DecodingTimestamp.Value >> 9));
                dataBuffer.Write((byte)(DecodingTimestamp.Value >> 1));
                dataBuffer.Write((byte)(((DecodingTimestamp.Value & 0x1) << 7) | 0x7e));
                dataBuffer.Write((byte)0x00);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteStuffing(IDataBuffer dataBuffer)
            {
                for (var i = 0; i < StuffingSize; i++)
                    dataBuffer.Write(TsConstants.StuffingByte);
            }
        }

        private record struct PacketizedElementaryStreamHeader(byte StreamId, uint DecodingTimestamp, uint PresentationTimestamp, int DataSize)
        {
            public int Size => 14 + (DecodingTimestamp != PresentationTimestamp ? 5 : 0);

            public void Write(IDataBuffer dataBuffer)
            {
                WritePrefix(dataBuffer);
                WriteStreamId(dataBuffer);
                WritePacketSizeAndFlagsAndTimestamps(dataBuffer);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void WritePrefix(IDataBuffer dataBuffer)
            {
                dataBuffer.Write((byte)0x00);
                dataBuffer.Write((byte)0x00);
                dataBuffer.Write((byte)0x01);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteStreamId(IDataBuffer dataBuffer)
            {
                dataBuffer.Write(StreamId);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WritePacketSizeAndFlagsAndTimestamps(IDataBuffer dataBuffer)
            {
                var headerSize = 5;
                var flags = 0x80;

                bool writeDts = DecodingTimestamp != PresentationTimestamp;
                if (writeDts)
                {
                    headerSize += 5;
                    flags |= 0x40;
                }

                var packetSize = DataSize + headerSize + 3;

                if (packetSize > 0xffff)
                    packetSize = 0;

                dataBuffer.Write((byte)(packetSize >> 8));
                dataBuffer.Write((byte)packetSize);

                dataBuffer.Write((byte)0x80);
                dataBuffer.Write((byte)flags);
                dataBuffer.Write((byte)headerSize);

                WriteTimestamp(dataBuffer, (byte)(flags >> 6), PresentationTimestamp);
                if (writeDts) WriteTimestamp(dataBuffer, 1, DecodingTimestamp);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteTimestamp(IDataBuffer dataBuffer, byte flags, uint timestamp)
            {
                uint val;

                val = (uint)(flags << 4) | ((timestamp >> 30) & 0x07) << 1 | 1;
                dataBuffer.Write((byte)val);

                val = (((timestamp >> 15) & 0x7fff) << 1) | 1;
                dataBuffer.Write((byte)(val >> 8));
                dataBuffer.Write((byte)val);

                val = ((timestamp & 0x7fff) << 1) | 1;
                dataBuffer.Write((byte)(val >> 8));
                dataBuffer.Write((byte)val);
            }
        }

        private record struct AudioDataTransportStreamHeader(AACSequenceHeader SequenceHeader, int DataSize)
        {
            public const int Size = 7;

            public void FillBuffer(byte[] buffer)
            {
                Debug.Assert(buffer.Length >= Size);

                var payloadLength = DataSize + Size;

                buffer[0] = 0xff;
                buffer[1] = 0xf1;
                buffer[2] = (byte)((SequenceHeader.ObjectType - 1) << 6 | SequenceHeader.SampleRateIndex << 2);
                buffer[3] = (byte)(SequenceHeader.ChannelConfig << 6 | (payloadLength >> 11) & 0x3);
                buffer[4] = (byte)(payloadLength >> 3);
                buffer[5] = (byte)(0x1f | payloadLength << 5);
                buffer[6] = 0xfc;
            }
        }

        private record struct ProgramSpecificInformationHeader(byte TableId, int DataSize)
        {
            public int Size => 4;
            public int ChecksumOffset => 1;

            public void Write(IDataBuffer dataBuffer)
            {
                WritePointerField(dataBuffer);
                WriteTableHeader(dataBuffer);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WritePointerField(IDataBuffer dataBuffer)
            {
                dataBuffer.Write((byte)0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteTableHeader(IDataBuffer dataBuffer)
            {
                dataBuffer.Write(TableId);
                dataBuffer.WriteUint16BigEndian((ushort)(0x1 << 15 | 0x3 << 12 | (DataSize + TableChecksum.Size)));
            }
        }

        private interface IPSITable
        {
            int Size { get; }
            void Write(IDataBuffer dataBuffer);
        }

        private record struct ProgramAssociationTable(ushort TransportStreamIdentifier, ushort ProgramNumber, ushort ProgramMapPID) : IPSITable
        {
            public int Size => 5 + 4;

            public void Write(IDataBuffer dataBuffer)
            {
                WriteTablePrefix(dataBuffer);
                WriteTable(dataBuffer);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteTablePrefix(IDataBuffer dataBuffer)
            {
                dataBuffer.WriteUint16BigEndian(TransportStreamIdentifier);
                dataBuffer.Write((byte)((0x3 << 6) | 1));
                dataBuffer.Write((byte)0x00);
                dataBuffer.Write((byte)0x00);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteTable(IDataBuffer dataBuffer)
            {
                dataBuffer.WriteUint16BigEndian(ProgramNumber);
                dataBuffer.WriteUint16BigEndian((ushort)(0xe000 | ProgramMapPID));
            }
        }

        private record struct ElementaryStreamInfo(byte StreamType, ushort ElementaryPID)
        {
            public int Size => 5;

            public void Write(IDataBuffer dataBuffer)
            {
                dataBuffer.Write(StreamType);
                dataBuffer.WriteUint16BigEndian((ushort)(0x07 << 13 | ElementaryPID));
                dataBuffer.WriteUint16BigEndian(0x0f << 12);
            }
        }

        private record struct ProgramMapTable(ushort ProgramNumber, ushort PCRPID, IList<ElementaryStreamInfo> ElementaryStreamInfos) : IPSITable
        {
            public int Size => 5 + 4 + ElementaryStreamInfos.Sum(x => x.Size);

            public void Write(IDataBuffer dataBuffer)
            {
                WriteTablePrefix(dataBuffer);
                WriteTable(dataBuffer);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteTablePrefix(IDataBuffer dataBuffer)
            {
                dataBuffer.WriteUint16BigEndian(ProgramNumber);
                dataBuffer.Write((byte)((0x3 << 6) | 1));
                dataBuffer.Write((byte)0x00);
                dataBuffer.Write((byte)0x00);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteTable(IDataBuffer dataBuffer)
            {
                dataBuffer.WriteUint16BigEndian((ushort)(0x07 << 13 | PCRPID));
                dataBuffer.WriteUint16BigEndian(0x0f << 12);

                foreach (var info in ElementaryStreamInfos)
                    info.Write(dataBuffer);
            }
        }

        private record struct TableChecksum(byte[] Buffer, int Start, int Length)
        {
            public const int Size = 4;

            public void Write(IDataBuffer dataBuffer)
            {
                var checksum = CRC32.Generate(Buffer, Start, Length);
                dataBuffer.WriteUInt32BigEndian(checksum);
            }
        }
    }
}
