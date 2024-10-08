﻿using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Media;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.Media
{
    public class RtmpAudioMessageHandlerTest : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpMediaMessageBroadcasterService _mediaMessageBroadcaster;
        private readonly ILogger<RtmpAudioMessageHandler> _logger;
        private readonly IDataBuffer _dataBuffer;
        private readonly RtmpAudioMessageHandler _sut;

        public RtmpAudioMessageHandlerTest()
        {
            _fixture = new Fixture();
            _clientContext = Substitute.For<IRtmpClientSessionContext>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _streamManager = Substitute.For<IRtmpStreamManagerService>();
            _mediaMessageCacher = Substitute.For<IRtmpMediaMessageCacherService>();
            _mediaMessageBroadcaster = Substitute.For<IRtmpMediaMessageBroadcasterService>();
            _logger = Substitute.For<ILogger<RtmpAudioMessageHandler>>();

            _dataBuffer = new DataBuffer();

            _sut = new RtmpAudioMessageHandler(
                _streamManager,
                _mediaMessageCacher,
                _mediaMessageBroadcaster,
                _logger);
        }

        public void Dispose()
        {
            _dataBuffer.Dispose();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnFalse_When_StreamNotYetCreated()
        {
            // Arrange
            _clientContext.GetStreamContext(Arg.Any<uint>()).Returns((IRtmpStreamContext?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, _dataBuffer, default);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(true, AudioCodec.AAC, AACPacketType.SequenceHeader)]
        [InlineData(true, AudioCodec.AAC, AACPacketType.Raw)]
        [InlineData(true, AudioCodec.Opus, AACPacketType.SequenceHeader)]
        [InlineData(true, AudioCodec.Opus, AACPacketType.Raw)]
        [InlineData(false, AudioCodec.AAC, AACPacketType.SequenceHeader)]
        [InlineData(false, AudioCodec.AAC, AACPacketType.Raw)]
        [InlineData(false, AudioCodec.Opus, AACPacketType.SequenceHeader)]
        [InlineData(false, AudioCodec.Opus, AACPacketType.Raw)]
        internal async Task HandleAsync_Should_HandleCacheAndBroadcastAndReturnTrue(
            bool gopCacheActivated, AudioCodec audioCodec, AACPacketType aacPacketType)
        {
            // Arrange
            var stremaPath = _fixture.Create<string>();
            var streamId = _fixture.Create<uint>();

            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscriber_subscribeStreamContexts = new List<IRtmpSubscribeStreamContext>() { subscriber_subscribeStreamContext };

            var publisher_streamContext = Substitute.For<IRtmpStreamContext>();
            var publisher_publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();

            publisher_streamContext.StreamId.Returns(streamId);
            publisher_streamContext.ClientContext.Returns(_clientContext);
            publisher_streamContext.PublishContext.Returns(publisher_publishStreamContext);
            publisher_publishStreamContext.StreamPath.Returns(stremaPath);
            publisher_publishStreamContext.StreamContext.Returns(publisher_streamContext);
            publisher_publishStreamContext.GroupOfPicturesCacheActivated.Returns(gopCacheActivated);

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);

            _clientContext.GetStreamContext(streamId).Returns(publisher_streamContext);
            _streamManager.GetSubscribeStreamContexts(stremaPath).Returns(subscriber_subscribeStreamContexts);

            var firstByte = (byte)((byte)audioCodec << 4);
            _dataBuffer.Write(firstByte);
            _dataBuffer.Write((byte)aacPacketType);
            _dataBuffer.Write(_fixture.Create<byte[]>());
            _dataBuffer.MoveTo(0);

            var hasHeader =
                (audioCodec is AudioCodec.AAC or AudioCodec.Opus) &&
                aacPacketType is AACPacketType.SequenceHeader;

            bool isPictureCachable = (audioCodec is AudioCodec.AAC or AudioCodec.Opus) && aacPacketType is not AACPacketType.SequenceHeader;

            var isSkippable = !hasHeader;

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, _dataBuffer, default);

            // Assert
            result.Should().BeTrue();

            _ = _mediaMessageCacher.Received(hasHeader ? 1 : 0)
                .CacheSequenceHeaderAsync(publisher_publishStreamContext, MediaType.Audio, _dataBuffer);

            _ = _mediaMessageCacher.Received(gopCacheActivated && isPictureCachable ? 1 : 0)
                .CachePictureAsync(publisher_publishStreamContext, MediaType.Audio, _dataBuffer, _chunkStreamContext.Timestamp);

            publisher_publishStreamContext.Received(1).UpdateTimestamp(_chunkStreamContext.Timestamp, MediaType.Audio);

            await _mediaMessageBroadcaster.Received(1).BroadcastMediaMessageAsync(
                publisher_publishStreamContext,
                subscriber_subscribeStreamContexts,
                MediaType.Audio,
                _chunkStreamContext.Timestamp,
                isSkippable,
                _dataBuffer
            );
        }
    }
}
