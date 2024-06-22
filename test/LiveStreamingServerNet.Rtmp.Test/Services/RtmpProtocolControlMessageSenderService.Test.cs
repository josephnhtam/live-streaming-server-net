using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
{
    public class RtmpProtocolControlMessageSenderServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly IRtmpProtocolControlMessageSenderService _sut;
        private readonly IRtmpClientContext _clientContext;
        private readonly IDataBuffer _payloadBuffer;

        public RtmpProtocolControlMessageSenderServiceTest()
        {
            _fixture = new Fixture();
            _chunkMessageSender = Substitute.For<IRtmpChunkMessageSenderService>();
            _sut = new RtmpProtocolControlMessageSenderService(_chunkMessageSender);

            _clientContext = Substitute.For<IRtmpClientContext>();
            _payloadBuffer = new DataBuffer();

            _chunkMessageSender.When(x => x.Send(
                _clientContext,
                Arg.Any<RtmpChunkBasicHeader>(),
                Arg.Any<RtmpChunkMessageHeaderType0>(),
                Arg.Any<Action<IDataBuffer>>(),
                Arg.Any<Action<bool>>()
            )).Do(x =>
            {
                x.Arg<Action<IDataBuffer>>().Invoke(_payloadBuffer);
                x.Arg<Action<bool>>()?.Invoke(true);
            });
        }

        [Fact]
        public void SetChunkSize_Should_SendChunkSizeMessageAndSetOutChunkSize()
        {
            // Arrange
            var outChunkSize = _fixture.Create<uint>();

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUInt32BigEndian(outChunkSize);

            // Act
            _sut.SetChunkSize(_clientContext, outChunkSize);

            // Assert
            _chunkMessageSender.Received(1).Send(
                _clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == RtmpConstants.ProtocolControlMessageChunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageStreamId == RtmpConstants.ProtocolControlMessageStreamId &&
                    x.MessageTypeId == RtmpMessageType.SetChunkSize
                ),
                Arg.Any<Action<IDataBuffer>>(),
                Arg.Any<Action<bool>>()
            );

            _payloadBuffer.UnderlyingBuffer.Take(_payloadBuffer.Size)
                .Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));

            _clientContext.Received(1).OutChunkSize = outChunkSize;
        }

        [Fact]
        public void AbortMessage_Should_SendAbortMessage()
        {
            // Arrange
            var streamId = _fixture.Create<uint>();

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUInt32BigEndian(streamId);

            // Act
            _sut.AbortMessage(_clientContext, streamId);

            // Assert
            _chunkMessageSender.Received(1).Send(
                _clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == RtmpConstants.ProtocolControlMessageChunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageStreamId == RtmpConstants.ProtocolControlMessageStreamId &&
                    x.MessageTypeId == RtmpMessageType.AbortMessage
                ),
                Arg.Any<Action<IDataBuffer>>(),
                Arg.Any<Action<bool>>()
            );

            _payloadBuffer.UnderlyingBuffer.Take(_payloadBuffer.Size)
                .Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));
        }

        [Fact]
        public void Acknowledgement_Should_SendAcknowledgementMessage()
        {
            // Arrange
            var sequenceNumber = _fixture.Create<uint>();

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUInt32BigEndian(sequenceNumber);

            // Act
            _sut.Acknowledgement(_clientContext, sequenceNumber);

            // Assert
            _chunkMessageSender.Received(1).Send(
                _clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == RtmpConstants.ProtocolControlMessageChunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageStreamId == RtmpConstants.ProtocolControlMessageStreamId &&
                    x.MessageTypeId == RtmpMessageType.Acknowledgement
                ),
                Arg.Any<Action<IDataBuffer>>(),
                Arg.Any<Action<bool>>()
            );

            _payloadBuffer.UnderlyingBuffer.Take(_payloadBuffer.Size)
                .Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));
        }

        [Fact]
        public void WindowAcknowledgementSize_Should_SendWindowAcknowledgementSizeMessageAndSetOutWindowAcknowledgementSize()
        {
            // Arrange
            var size = _fixture.Create<uint>();

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUInt32BigEndian(size);

            // Act
            _sut.WindowAcknowledgementSize(_clientContext, size);

            // Assert
            _chunkMessageSender.Received(1).Send(
                _clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == RtmpConstants.ProtocolControlMessageChunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageStreamId == RtmpConstants.ProtocolControlMessageStreamId &&
                    x.MessageTypeId == RtmpMessageType.WindowAcknowledgementSize
                ),
                Arg.Any<Action<IDataBuffer>>(),
                Arg.Any<Action<bool>>()
            );

            _payloadBuffer.UnderlyingBuffer.Take(_payloadBuffer.Size)
                .Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));

            _clientContext.Received(1).OutWindowAcknowledgementSize = size;
        }

        [Fact]
        public void SetClientBandwidth_Should_SendSetClientBandwidthMessageAndSetOutBandwidth()
        {
            // Arrange
            var bandwidth = _fixture.Create<uint>();
            var limitType = _fixture.Create<RtmpClientBandwidthLimitType>();

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUInt32BigEndian(bandwidth);
            expectedBuffer.Write((byte)limitType);

            // Act
            _sut.SetClientBandwidth(_clientContext, bandwidth, limitType);

            // Assert
            _chunkMessageSender.Received(1).Send(
                _clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == RtmpConstants.ProtocolControlMessageChunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageStreamId == RtmpConstants.ProtocolControlMessageStreamId &&
                    x.MessageTypeId == RtmpMessageType.SetClientBandwidth
                ),
                Arg.Any<Action<IDataBuffer>>(),
                Arg.Any<Action<bool>>()
            );

            _payloadBuffer.UnderlyingBuffer.Take(_payloadBuffer.Size)
                .Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));
        }
    }
}
