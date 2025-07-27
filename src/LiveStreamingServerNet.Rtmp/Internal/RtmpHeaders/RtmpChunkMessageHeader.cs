﻿using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders
{
    internal interface IRtmpChunkMessageHeader
    {
        int Size { get; }
        void Write(IDataBuffer dataBuffer);
        bool HasExtendedTimestamp();
        uint GetTimestamp();
        void SetMessageLength(int messageLength);
    }

    internal record struct RtmpChunkMessageHeaderType0 : IRtmpChunkMessageHeader
    {
        public const int kSize = 11;
        public int Size => kSize;

        private const int ExtendedTimestamp = 0xffffff;

        public uint Timestamp { get; }
        public int MessageLength { get; private set; }
        public byte MessageTypeId { get; }
        public uint MessageStreamId { get; }


        public RtmpChunkMessageHeaderType0(uint timestamp, int messageLength, byte messageTypeId, uint messageStreamId)
        {
            Timestamp = timestamp;
            MessageLength = messageLength;
            MessageTypeId = messageTypeId;
            MessageStreamId = messageStreamId;
        }

        public RtmpChunkMessageHeaderType0(uint timestamp, byte messageTypeId, uint messageStreamId)
        {
            Timestamp = timestamp;
            MessageLength = 0;
            MessageTypeId = messageTypeId;
            MessageStreamId = messageStreamId;
        }

        public static async ValueTask<RtmpChunkMessageHeaderType0> ReadAsync(IDataBuffer dataBuffer, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            await dataBuffer.FromStreamData(networkStream, kSize, cancellationToken).ConfigureAwait(false);
            var timestampDelta = dataBuffer.ReadUInt24BigEndian();
            var messageLength = (int)dataBuffer.ReadUInt24BigEndian();
            var messageTypeId = dataBuffer.ReadByte();
            var messageStreamId = dataBuffer.ReadUInt32();

            return new RtmpChunkMessageHeaderType0(timestampDelta, messageLength, messageTypeId, messageStreamId);
        }

        public void SetMessageLength(int messageLength)
        {
            MessageLength = messageLength;
        }

        public bool HasExtendedTimestamp()
        {
            return Timestamp >= ExtendedTimestamp;
        }

        public uint GetTimestamp()
        {
            return Timestamp;
        }

        public void Write(IDataBuffer dataBuffer)
        {
            dataBuffer.WriteUInt24BigEndian(HasExtendedTimestamp() ? ExtendedTimestamp : Timestamp);
            dataBuffer.WriteUInt24BigEndian((uint)MessageLength);
            dataBuffer.Write(MessageTypeId);
            dataBuffer.Write(MessageStreamId);
        }
    }

    internal record struct RtmpChunkMessageHeaderType1 : IRtmpChunkMessageHeader
    {
        public const int kSize = 7;
        public int Size => kSize;

        private const int ExtendedTimestamp = 0xffffff;

        public uint TimestampDelta { get; }
        public int MessageLength { get; private set; }
        public byte MessageTypeId { get; }


        public RtmpChunkMessageHeaderType1(uint timestampDelta, int messageLength, byte messageTypeId)
        {
            TimestampDelta = timestampDelta;
            MessageLength = messageLength;
            MessageTypeId = messageTypeId;
        }

        public RtmpChunkMessageHeaderType1(uint timestampDelta, byte messageTypeId)
        {
            TimestampDelta = timestampDelta;
            MessageLength = 0;
            MessageTypeId = messageTypeId;
        }

        public static async ValueTask<RtmpChunkMessageHeaderType1> ReadAsync(IDataBuffer dataBuffer, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            await dataBuffer.FromStreamData(networkStream, kSize, cancellationToken).ConfigureAwait(false);

            var timestampDelta = dataBuffer.ReadUInt24BigEndian();
            var messageLength = (int)dataBuffer.ReadUInt24BigEndian();
            var messageTypeId = dataBuffer.ReadByte();

            return new RtmpChunkMessageHeaderType1(timestampDelta, messageLength, messageTypeId);
        }

        public void SetMessageLength(int messageLength)
        {
            MessageLength = messageLength;
        }

        public bool HasExtendedTimestamp()
        {
            return TimestampDelta >= ExtendedTimestamp;
        }

        public uint GetTimestamp()
        {
            return TimestampDelta;
        }

        public void Write(IDataBuffer dataBuffer)
        {
            dataBuffer.WriteUInt24BigEndian(HasExtendedTimestamp() ? ExtendedTimestamp : TimestampDelta);
            dataBuffer.WriteUInt24BigEndian((uint)MessageLength);
            dataBuffer.Write(MessageTypeId);
        }
    }

    internal record struct RtmpChunkMessageHeaderType2 : IRtmpChunkMessageHeader
    {
        public const int kSize = 3;
        public int Size => kSize;

        private const int ExtendedTimestamp = 0xffffff;

        public uint TimestampDelta { get; }

        public RtmpChunkMessageHeaderType2(uint TimestampDelta)
        {
            this.TimestampDelta = TimestampDelta;
        }

        public static async ValueTask<RtmpChunkMessageHeaderType2> ReadAsync(IDataBuffer dataBuffer, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            await dataBuffer.FromStreamData(networkStream, kSize, cancellationToken).ConfigureAwait(false);

            var timestampDelta = dataBuffer.ReadUInt24BigEndian();

            return new RtmpChunkMessageHeaderType2(timestampDelta);
        }

        public void SetMessageLength(int messageLength) { }

        public bool HasExtendedTimestamp()
        {
            return TimestampDelta >= ExtendedTimestamp;
        }

        public uint GetTimestamp()
        {
            return TimestampDelta;
        }

        public void Write(IDataBuffer dataBuffer)
        {
            dataBuffer.WriteUInt24BigEndian(HasExtendedTimestamp() ? ExtendedTimestamp : TimestampDelta);
        }
    }
}
