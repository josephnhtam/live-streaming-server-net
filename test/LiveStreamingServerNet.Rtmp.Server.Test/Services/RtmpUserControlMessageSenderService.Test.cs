using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.Services
{
    public class RtmpUserControlMessageSenderServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly IRtmpUserControlMessageSenderService _sut;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IRtmpStreamContext _streamContext;
        private readonly IRtmpSubscribeStreamContext _subscribeStreamContext;
        private readonly DataBuffer _payloadBuffer;

        public RtmpUserControlMessageSenderServiceTest()
        {
            _fixture = new Fixture();
            _chunkMessageSender = Substitute.For<IRtmpChunkMessageSenderService>();
            _sut = new RtmpUserControlMessageSenderService(_chunkMessageSender);

            _clientContext = Substitute.For<IRtmpClientSessionContext>();
            _streamContext = Substitute.For<IRtmpStreamContext>();
            _subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            _payloadBuffer = new DataBuffer();

            _subscribeStreamContext.StreamContext.Returns(_streamContext);
            _streamContext.SubscribeContext.Returns(_subscribeStreamContext);
            _streamContext.ClientContext.Returns(_clientContext);

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
                Arg.Any<IReadOnlyList<IRtmpClientSessionContext>>(),
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

            _streamContext.StreamId.Returns(streamId);

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
            expectedBuffer.WriteUInt32BigEndian(streamId);

            // Act
            _sut.SendStreamBeginMessage(_subscribeStreamContext);

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
            _streamContext.StreamId.Returns(0u);
            var subscriberStreamContexts = new List<IRtmpSubscribeStreamContext> { _subscribeStreamContext };

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
            expectedBuffer.WriteUInt32BigEndian(0u);

            // Act
            _sut.SendStreamBeginMessage(subscriberStreamContexts);

            // Assert
            _chunkMessageSender.Received(1).Send(
                Arg.Is<IReadOnlyList<IRtmpClientSessionContext>>(x => x.Contains(_clientContext)),
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
            _streamContext.StreamId.Returns(streamId);

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
            expectedBuffer.WriteUInt32BigEndian(streamId);

            // Act
            _sut.SendStreamEofMessage(_subscribeStreamContext);

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
            _streamContext.StreamId.Returns(0u);
            var subscriberStreamContexts = new List<IRtmpSubscribeStreamContext> { _subscribeStreamContext };

            using var expectedBuffer = new DataBuffer();
            expectedBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
            expectedBuffer.WriteUInt32BigEndian(0u);

            // Act
            _sut.SendStreamEofMessage(subscriberStreamContexts);

            // Assert
            _chunkMessageSender.Received(1).Send(
                Arg.Is<IReadOnlyList<IRtmpClientSessionContext>>(x => x.Contains(_clientContext)),
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
