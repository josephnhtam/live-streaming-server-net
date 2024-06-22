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
    public class RtmpUserControlMessageSenderServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly IRtmpUserControlMessageSenderService _sut;
        private readonly IRtmpClientContext _clientContext;
        private readonly DataBuffer _payloadBuffer;

        public RtmpUserControlMessageSenderServiceTest()
        {
            _fixture = new Fixture();
            _chunkMessageSender = Substitute.For<IRtmpChunkMessageSenderService>();
            _sut = new RtmpUserControlMessageSenderService(_chunkMessageSender);

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

            _chunkMessageSender.When(x => x.Send(
                Arg.Any<IReadOnlyList<IRtmpClientContext>>(),
                Arg.Any<RtmpChunkBasicHeader>(),
                Arg.Any<RtmpChunkMessageHeaderType0>(),
                Arg.Any<Action<IDataBuffer>>()
            )).Do(x =>
            {
                x.Arg<Action<IDataBuffer>>().Invoke(_payloadBuffer);
            });
        }

        [Fact]
        public void SendStreamBeginMessage_Should_SendStreamBeginMessage()
        {
            // Arrange
            var streamId = _fixture.Create<uint>();
            _clientContext.StreamSubscriptionContext!.StreamId.Returns(streamId);

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
            expectedBuffer.WriteUInt32BigEndian(streamId);

            // Act
            _sut.SendStreamBeginMessage(_clientContext);

            // Assert
            _chunkMessageSender.Received(1).Send(
                _clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == RtmpConstants.UserControlMessageChunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageStreamId == RtmpConstants.UserControlMessageStreamId &&
                    x.MessageTypeId == RtmpMessageType.UserControlMessage
                ),
                Arg.Any<Action<IDataBuffer>>(),
                Arg.Any<Action<bool>>()
            );

            _payloadBuffer.UnderlyingBuffer.Take(_payloadBuffer.Size)
                .Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));
        }


        [Fact]
        public void SendStreamBeginMessage_Should_BroadcastStreamBeginMessage()
        {
            // Arrange
            _clientContext.StreamSubscriptionContext!.StreamId.Returns(0u);
            var clientContexts = new List<IRtmpClientContext> { _clientContext };

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
            expectedBuffer.WriteUInt32BigEndian(0u);

            // Act
            _sut.SendStreamBeginMessage(clientContexts);

            // Assert
            _chunkMessageSender.Received(1).Send(
                Arg.Is<IReadOnlyList<IRtmpClientContext>>(x => x.Contains(_clientContext)),
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == RtmpConstants.UserControlMessageChunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageStreamId == RtmpConstants.UserControlMessageStreamId &&
                    x.MessageTypeId == RtmpMessageType.UserControlMessage
                ),
                Arg.Any<Action<IDataBuffer>>()
            );

            _payloadBuffer.UnderlyingBuffer.Take(_payloadBuffer.Size)
                .Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));
        }

        [Fact]
        public void SendStreamEofMessage_Should_SendStreamEofMessage()
        {
            // Arrange
            var streamId = _fixture.Create<uint>();
            _clientContext.StreamSubscriptionContext!.StreamId.Returns(streamId);

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
            expectedBuffer.WriteUInt32BigEndian(streamId);

            // Act
            _sut.SendStreamEofMessage(_clientContext);

            // Assert
            _chunkMessageSender.Received(1).Send(
                _clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == RtmpConstants.UserControlMessageChunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageStreamId == RtmpConstants.UserControlMessageStreamId &&
                    x.MessageTypeId == RtmpMessageType.UserControlMessage
                ),
                Arg.Any<Action<IDataBuffer>>(),
                Arg.Any<Action<bool>>()
            );

            _payloadBuffer.UnderlyingBuffer.Take(_payloadBuffer.Size)
                .Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));
        }

        [Fact]
        public void SendStreamEofMessage_Should_BroadcastStreamEofMessage()
        {
            // Arrange
            _clientContext.StreamSubscriptionContext!.StreamId.Returns(0u);
            var clientContexts = new List<IRtmpClientContext> { _clientContext };

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
            expectedBuffer.WriteUInt32BigEndian(0u);

            // Act
            _sut.SendStreamEofMessage(clientContexts);

            // Assert
            _chunkMessageSender.Received(1).Send(
                Arg.Is<IReadOnlyList<IRtmpClientContext>>(x => x.Contains(_clientContext)),
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == RtmpConstants.UserControlMessageChunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageStreamId == RtmpConstants.UserControlMessageStreamId &&
                    x.MessageTypeId == RtmpMessageType.UserControlMessage
                ),
                Arg.Any<Action<IDataBuffer>>()
            );

            _payloadBuffer.UnderlyingBuffer.Take(_payloadBuffer.Size)
                .Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));
        }
    }
}
