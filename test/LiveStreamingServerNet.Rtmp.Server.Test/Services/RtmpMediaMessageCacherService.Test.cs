using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.Services
{
    public class RtmpMediaMessageCacherServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly IRtmpMediaCachingInterceptionService _interception;
        private readonly MediaMessageConfiguration _config;
        private readonly ILogger<RtmpMediaMessageCacherService> _logger;
        private readonly IRtmpMediaMessageCacherService _sut;

        public RtmpMediaMessageCacherServiceTest()
        {
            _fixture = new Fixture();
            _chunkMessageSender = Substitute.For<IRtmpChunkMessageSenderService>();
            _interception = Substitute.For<IRtmpMediaCachingInterceptionService>();
            _config = new MediaMessageConfiguration();
            _logger = Substitute.For<ILogger<RtmpMediaMessageCacherService>>();

            _sut = new RtmpMediaMessageCacherService(
                _chunkMessageSender,
                _interception,
                Options.Create(_config),
                _logger);
        }

        [Fact]
        public async Task CacheSequenceHeaderAsync_Should_SetVideoSequenceHeader_When_MediaTypeIsVideo()
        {
            // Arrange
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var mediaType = MediaType.Video;
            var payloadBuffer = Substitute.For<IDataBuffer>();
            var sequenceHeader = _fixture.Create<byte[]>();

            payloadBuffer.MoveTo(0).Returns(payloadBuffer);
            payloadBuffer.Size.Returns(sequenceHeader.Length);
            payloadBuffer.ReadBytes(payloadBuffer.Size).Returns(sequenceHeader);

            // Act
            await _sut.CacheSequenceHeaderAsync(publishStreamContext, mediaType, payloadBuffer);

            // Assert
            await _interception.Received(1).CacheSequenceHeaderAsync(publishStreamContext.StreamPath, mediaType, sequenceHeader);
            publishStreamContext.VideoSequenceHeader.Should().BeEquivalentTo(sequenceHeader);
        }

        [Fact]
        public async Task CacheSequenceHeaderAsync_Should_SetAudioSequenceHeader_When_MediaTypeIsAudio()
        {
            // Arrange
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var mediaType = MediaType.Audio;
            var payloadBuffer = Substitute.For<IDataBuffer>();
            var sequenceHeader = _fixture.Create<byte[]>();

            payloadBuffer.MoveTo(0).Returns(payloadBuffer);
            payloadBuffer.Size.Returns(sequenceHeader.Length);
            payloadBuffer.ReadBytes(payloadBuffer.Size).Returns(sequenceHeader);

            // Act
            await _sut.CacheSequenceHeaderAsync(publishStreamContext, mediaType, payloadBuffer);

            // Assert
            await _interception.Received(1).CacheSequenceHeaderAsync(publishStreamContext.StreamPath, mediaType, sequenceHeader);
            publishStreamContext.AudioSequenceHeader.Should().BeEquivalentTo(sequenceHeader);
        }

        [Fact]
        public async Task CachePictureAsync_Should_AddPictureToGroupOfPicturesCache()
        {
            // Arrange
            var payload = _fixture.Create<byte[]>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            publishStreamContext.GroupOfPicturesCache.Size.Returns(_config.MaxGroupOfPicturesCacheSize - 1);

            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();

            var payloadBuffer = new DataBuffer();
            payloadBuffer.Write(payload);
            payloadBuffer.MoveTo(0);

            PictureCacheInfo? pictureCacheInfo = null;
            byte[]? pictureCacheBuffer = null;
            publishStreamContext.GroupOfPicturesCache.When(x => x.Add(Arg.Any<PictureCacheInfo>(), payloadBuffer))
                .Do(x =>
                {
                    pictureCacheInfo = x.Arg<PictureCacheInfo>();
                    pictureCacheBuffer = x.Arg<IDataBuffer>().ReadBytes(x.Arg<IDataBuffer>().Size);
                });

            // Act
            await _sut.CachePictureAsync(publishStreamContext, mediaType, payloadBuffer, timestamp);

            // Assert
            await _interception.Received(1).CachePictureAsync(
                publishStreamContext.StreamPath,
                mediaType,
                Arg.Any<IDataBuffer>(),
                timestamp);

            publishStreamContext.GroupOfPicturesCache.Received(1).Add(Arg.Any<PictureCacheInfo>(), Arg.Any<IDataBuffer>());

            pictureCacheInfo.Should().NotBeNull();
            pictureCacheBuffer.Should().NotBeNull();

            pictureCacheBuffer!.Should().BeEquivalentTo(payload);

            pictureCacheInfo!.Value.Type.Should().Be(mediaType);
            pictureCacheInfo!.Value.Timestamp.Should().Be(timestamp);
        }

        [Fact]
        public async Task CachePictureAsync_Should_ClearsGroupOfPicturesCache_When_GroupOfPicturesCacheIsFull()
        {
            // Arrange
            var payload = _fixture.Create<byte[]>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            publishStreamContext.GroupOfPicturesCache.Size.Returns(_config.MaxGroupOfPicturesCacheSize);

            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();

            var payloadBuffer = new DataBuffer();
            payloadBuffer.Write(payload);
            payloadBuffer.MoveTo(0);

            PictureCacheInfo? pictureCacheInfo = null;
            byte[]? pictureCacheBuffer = null;
            publishStreamContext.GroupOfPicturesCache.When(x => x.Add(Arg.Any<PictureCacheInfo>(), Arg.Any<IDataBuffer>()))
                .Do(x =>
                {
                    pictureCacheInfo = x.Arg<PictureCacheInfo>();
                    pictureCacheBuffer = x.Arg<IDataBuffer>().ReadBytes(x.Arg<IDataBuffer>().Size);
                });

            // Act
            await _sut.CachePictureAsync(publishStreamContext, mediaType, payloadBuffer, timestamp);

            // Assert
            publishStreamContext.GroupOfPicturesCache.Received(1).Clear();

            await _interception.Received(1).CachePictureAsync(
                publishStreamContext.StreamPath, mediaType, Arg.Any<IDataBuffer>(), timestamp);

            publishStreamContext.GroupOfPicturesCache.Received(1).Add(Arg.Any<PictureCacheInfo>(), Arg.Any<IDataBuffer>());

            pictureCacheInfo.Should().NotBeNull();
            pictureCacheBuffer.Should().NotBeNull();

            pictureCacheBuffer!.Should().BeEquivalentTo(payload);

            pictureCacheInfo!.Value.Type.Should().Be(mediaType);
            pictureCacheInfo!.Value.Timestamp.Should().Be(timestamp);
        }

        [Fact]
        public async Task ClearGroupOfPicturesCacheAsync_Should_ClearGroupOfPicturesCache()
        {
            // Arrange
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();

            // Act
            await _sut.ClearGroupOfPicturesCacheAsync(publishStreamContext);

            // Assert
            await _interception.Received(1).ClearGroupOfPicturesCacheAsync(publishStreamContext.StreamPath);
            publishStreamContext.GroupOfPicturesCache.Received(1).Clear();
        }

        [Fact]
        public void SendCachedHeaderMessages_Should_SendAudioSequenceHeader_When_AudioSequenceHeaderIsNotNull()
        {
            // Arrange
            var subscriber_clientContext = Substitute.For<IRtmpClientSessionContext>();
            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscriber_streamContext = Substitute.For<IRtmpStreamContext>();
            var publisher_streamContext = Substitute.For<IRtmpPublishStreamContext>();
            var audioSequenceHeader = _fixture.Create<byte[]>();
            var streamId = _fixture.Create<uint>();

            subscriber_subscribeStreamContext.StreamContext.Returns(subscriber_streamContext);
            subscriber_subscribeStreamContext.UpdateTimestamp(Arg.Any<uint>(), Arg.Any<MediaType>()).Returns(true);
            subscriber_streamContext.StreamId.Returns(streamId);
            subscriber_streamContext.ClientContext.Returns(subscriber_clientContext);

            publisher_streamContext.AudioSequenceHeader.Returns(audioSequenceHeader);

            using var audioBuffer = new DataBuffer();
            _chunkMessageSender.When(
                x => x.Send(
                    subscriber_clientContext,
                    Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == subscriber_subscribeStreamContext.AudioChunkStreamId),
                    Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.AudioMessage),
                    Arg.Any<Action<IDataBuffer>>()
                )
            ).Do(x => x.Arg<Action<IDataBuffer>>().Invoke(audioBuffer));

            // Act
            _sut.SendCachedHeaderMessages(subscriber_subscribeStreamContext, publisher_streamContext);

            // Assert
            _chunkMessageSender.Received(1).Send(
                subscriber_clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == subscriber_subscribeStreamContext.AudioChunkStreamId),
                Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.AudioMessage),
                Arg.Any<Action<IDataBuffer>>());

            audioBuffer.UnderlyingBuffer.Take(audioBuffer.Size).Should().BeEquivalentTo(audioSequenceHeader);
        }

        [Fact]
        public void SendCachedHeaderMessages_Should_SendVideoSequenceHeader_When_VideoSequenceHeaderIsNotNull()
        {
            // Arrange
            var subscriber_clientContext = Substitute.For<IRtmpClientSessionContext>();
            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscriber_streamContext = Substitute.For<IRtmpStreamContext>();
            var publishr_streamContext = Substitute.For<IRtmpPublishStreamContext>();
            var videoSequenceHeader = _fixture.Create<byte[]>();
            var streamId = _fixture.Create<uint>();

            subscriber_subscribeStreamContext.StreamContext.Returns(subscriber_streamContext);
            subscriber_subscribeStreamContext.UpdateTimestamp(Arg.Any<uint>(), Arg.Any<MediaType>()).Returns(true);
            subscriber_streamContext.StreamId.Returns(streamId);
            subscriber_streamContext.ClientContext.Returns(subscriber_clientContext);

            publishr_streamContext.VideoSequenceHeader.Returns(videoSequenceHeader);

            using var videoBuffer = new DataBuffer();
            _chunkMessageSender.When(
                x => x.Send(
                    subscriber_clientContext,
                    Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == subscriber_subscribeStreamContext.VideoChunkStreamId),
                    Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.VideoMessage),
                    Arg.Any<Action<IDataBuffer>>()
                )
            ).Do(x => x.Arg<Action<IDataBuffer>>().Invoke(videoBuffer));

            // Act
            _sut.SendCachedHeaderMessages(subscriber_subscribeStreamContext, publishr_streamContext);

            // Assert
            _chunkMessageSender.Received(1).Send(
               subscriber_clientContext,
               Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == subscriber_subscribeStreamContext.VideoChunkStreamId),
               Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.VideoMessage),
               Arg.Any<Action<IDataBuffer>>());

            videoBuffer.UnderlyingBuffer.Take(videoBuffer.Size).Should().BeEquivalentTo(videoSequenceHeader);
        }

        [Fact]
        public void SendCachedStreamMetaDataMessage_Should_SendStreamMetaDataMessage()
        {
            // Arrange
            var subscriber_clientContext = Substitute.For<IRtmpClientSessionContext>();
            var publisher_publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscriber_streamContext = Substitute.For<IRtmpStreamContext>();
            var streamMetaData = _fixture.Create<Dictionary<string, object>>();
            var timestamp = _fixture.Create<uint>();
            var streamId = _fixture.Create<uint>();

            subscriber_subscribeStreamContext.StreamContext.Returns(subscriber_streamContext);
            subscriber_subscribeStreamContext.UpdateTimestamp(Arg.Any<uint>(), Arg.Any<MediaType>()).Returns(true);
            subscriber_streamContext.StreamId.Returns(streamId);
            subscriber_streamContext.ClientContext.Returns(subscriber_clientContext);

            publisher_publishStreamContext.StreamMetaData.Returns(streamMetaData);

            using var metaDataBuffer = new DataBuffer();
            _chunkMessageSender.When(
                x => x.Send(
                    subscriber_clientContext,
                    Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == subscriber_subscribeStreamContext.DataChunkStreamId),
                    Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.DataMessageAmf0),
                    Arg.Any<Action<IDataBuffer>>()
                )
            ).Do(x => x.Arg<Action<IDataBuffer>>().Invoke(metaDataBuffer));

            // Act
            _sut.SendCachedStreamMetaDataMessage(subscriber_subscribeStreamContext, publisher_publishStreamContext, timestamp);

            // Assert
            _chunkMessageSender.Received(1).Send(
                subscriber_clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == subscriber_subscribeStreamContext.DataChunkStreamId),
                Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.DataMessageAmf0),
                Arg.Any<Action<IDataBuffer>>());

            var amf = metaDataBuffer.MoveTo(0).ReadAmf(metaDataBuffer.Size, AmfEncodingType.Amf0);
            amf[0].Should().Be(RtmpDataMessageConstants.OnMetaData);
            amf[1].Should().BeEquivalentTo(streamMetaData);
        }

        [Fact]
        public void SendCachedStreamMetaDataMessage_Should_BroadcastStreamMetaDataMessage()
        {
            // Arrange
            var subscriber_clientContext = Substitute.For<IRtmpClientSessionContext>();
            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscriber_streamContext = Substitute.For<IRtmpStreamContext>();
            var publisher_publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var streamMetaData = _fixture.Create<Dictionary<string, object>>();
            var timestamp = _fixture.Create<uint>();
            var streamId = _fixture.Create<uint>();

            subscriber_subscribeStreamContext.StreamContext.Returns(subscriber_streamContext);
            subscriber_subscribeStreamContext.UpdateTimestamp(Arg.Any<uint>(), Arg.Any<MediaType>()).Returns(true);
            subscriber_streamContext.StreamId.Returns(streamId);
            subscriber_streamContext.ClientContext.Returns(subscriber_clientContext);

            publisher_publishStreamContext.StreamMetaData.Returns(streamMetaData);

            using var metaDataBuffer = new DataBuffer();
            _chunkMessageSender.When(
                x => x.Send(
                    Arg.Is<List<IRtmpClientSessionContext>>(x => x.Contains(subscriber_clientContext)),
                    Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == subscriber_subscribeStreamContext.DataChunkStreamId),
                    Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.DataMessageAmf0),
                    Arg.Any<Action<IDataBuffer>>()
                )
            ).Do(x => x.Arg<Action<IDataBuffer>>().Invoke(metaDataBuffer));

            // Act
            var subscribeStreamContexts = new List<IRtmpSubscribeStreamContext> { subscriber_subscribeStreamContext };
            _sut.SendCachedStreamMetaDataMessage(subscribeStreamContexts, publisher_publishStreamContext, timestamp);

            // Assert
            _chunkMessageSender.Received(1).Send(
                Arg.Is<List<IRtmpClientSessionContext>>(x => x.Contains(subscriber_clientContext)),
                Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == subscriber_subscribeStreamContext.DataChunkStreamId),
                Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.DataMessageAmf0),
                Arg.Any<Action<IDataBuffer>>());

            var amf = metaDataBuffer.MoveTo(0).ReadAmf(metaDataBuffer.Size, AmfEncodingType.Amf0);
            amf[0].Should().Be(RtmpDataMessageConstants.OnMetaData);
            amf[1].Should().BeEquivalentTo(streamMetaData);
        }

        [Fact]
        public void SendCachedGroupOfPictures_Should_SendGroupOfPictures()
        {
            // Arrange
            var subscriber_clientContext = Substitute.For<IRtmpClientSessionContext>();
            var publisher_publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscriber_streamContext = Substitute.For<IRtmpStreamContext>();
            var streamId = _fixture.Create<uint>();
            var outChunkSize = _fixture.Create<uint>();

            subscriber_clientContext.OutChunkSize.Returns(outChunkSize);
            subscriber_subscribeStreamContext.StreamContext.Returns(subscriber_streamContext);
            subscriber_subscribeStreamContext.UpdateTimestamp(Arg.Any<uint>(), Arg.Any<MediaType>()).Returns(true);
            subscriber_streamContext.StreamId.Returns(streamId);
            subscriber_streamContext.ClientContext.Returns(subscriber_clientContext);

            var pictureCache1 = new PictureCache(
                _fixture.Create<MediaType>(),
                1,
                new RentedBuffer(_fixture.Create<int>())
            );

            var pictureCache2 = new PictureCache(
                _fixture.Create<MediaType>(),
                2,
                new RentedBuffer(_fixture.Create<int>())
            );

            var groupOfPictures = new List<PictureCache> { pictureCache1, pictureCache2 };

            publisher_publishStreamContext.GroupOfPicturesCache.Get().Returns(groupOfPictures);

            using var payloadsBuffer = new DataBuffer();
            _chunkMessageSender.When(
                x => x.Send(
                    subscriber_clientContext,
                    Arg.Any<RtmpChunkBasicHeader>(),
                    Arg.Any<RtmpChunkMessageHeaderType0>(),
                    Arg.Any<Action<IDataBuffer>>()
                )
            ).Do(x => x.Arg<Action<IDataBuffer>>().Invoke(payloadsBuffer));

            var expectedPayloadsBufffer =
                pictureCache1.Payload.Buffer.Take(pictureCache1.Payload.Size)
                .Concat(pictureCache2.Payload.Buffer.Take(pictureCache2.Payload.Size))
                .ToArray();

            // Act
            _sut.SendCachedGroupOfPictures(subscriber_subscribeStreamContext, publisher_publishStreamContext);

            // Assert
            _chunkMessageSender.Received(groupOfPictures.Count).Send(
                subscriber_clientContext,
                Arg.Any<RtmpChunkBasicHeader>(),
                Arg.Any<RtmpChunkMessageHeaderType0>(),
                Arg.Any<Action<IDataBuffer>>());

            payloadsBuffer.UnderlyingBuffer.Take(payloadsBuffer.Size).Should().BeEquivalentTo(expectedPayloadsBufffer);
        }
    }
}
