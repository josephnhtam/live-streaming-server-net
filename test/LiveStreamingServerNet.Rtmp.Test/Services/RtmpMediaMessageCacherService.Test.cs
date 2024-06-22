using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
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
            var clientContext = Substitute.For<IRtmpClientContext>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var audioSequenceHeader = _fixture.Create<byte[]>();
            var streamId = _fixture.Create<uint>();

            publishStreamContext.AudioSequenceHeader.Returns(audioSequenceHeader);
            clientContext.UpdateTimestamp(Arg.Any<uint>(), Arg.Any<MediaType>()).Returns(true);

            using var audioBuffer = new DataBuffer();
            _chunkMessageSender.When(
                x => x.Send(
                    clientContext,
                    Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == RtmpConstants.AudioMessageChunkStreamId),
                    Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.AudioMessage),
                    Arg.Any<Action<IDataBuffer>>()
                )
            ).Do(x => x.Arg<Action<IDataBuffer>>().Invoke(audioBuffer));

            // Act
            _sut.SendCachedHeaderMessages(clientContext, publishStreamContext, streamId);

            // Assert
            _chunkMessageSender.Received(1).Send(
                clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == RtmpConstants.AudioMessageChunkStreamId),
                Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.AudioMessage),
                Arg.Any<Action<IDataBuffer>>());

            audioBuffer.UnderlyingBuffer.Take(audioBuffer.Size).Should().BeEquivalentTo(audioSequenceHeader);
        }

        [Fact]
        public void SendCachedHeaderMessages_Should_SendVideoSequenceHeader_When_VideoSequenceHeaderIsNotNull()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientContext>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var videoSequenceHeader = _fixture.Create<byte[]>();
            var streamId = _fixture.Create<uint>();

            publishStreamContext.VideoSequenceHeader.Returns(videoSequenceHeader);
            clientContext.UpdateTimestamp(Arg.Any<uint>(), Arg.Any<MediaType>()).Returns(true);

            using var videoBuffer = new DataBuffer();
            _chunkMessageSender.When(
                x => x.Send(
                    clientContext,
                    Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == RtmpConstants.VideoMessageChunkStreamId),
                    Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.VideoMessage),
                    Arg.Any<Action<IDataBuffer>>()
                )
            ).Do(x => x.Arg<Action<IDataBuffer>>().Invoke(videoBuffer));

            // Act
            _sut.SendCachedHeaderMessages(clientContext, publishStreamContext, streamId);

            // Assert
            _chunkMessageSender.Received(1).Send(
               clientContext,
               Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == RtmpConstants.VideoMessageChunkStreamId),
               Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.VideoMessage),
               Arg.Any<Action<IDataBuffer>>());

            videoBuffer.UnderlyingBuffer.Take(videoBuffer.Size).Should().BeEquivalentTo(videoSequenceHeader);
        }

        [Fact]
        public void SendCachedStreamMetaDataMessage_Should_SendStreamMetaDataMessage()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientContext>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var streamMetaData = _fixture.Create<Dictionary<string, object>>();
            var timestamp = _fixture.Create<uint>();
            var streamId = _fixture.Create<uint>();

            publishStreamContext.StreamMetaData.Returns(streamMetaData);

            using var metaDataBuffer = new DataBuffer();
            _chunkMessageSender.When(
                x => x.Send(
                    clientContext,
                    Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == RtmpConstants.DataMessageChunkStreamId),
                    Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.DataMessageAmf0),
                    Arg.Any<Action<IDataBuffer>>()
                )
            ).Do(x => x.Arg<Action<IDataBuffer>>().Invoke(metaDataBuffer));

            // Act
            _sut.SendCachedStreamMetaDataMessage(clientContext, publishStreamContext, timestamp, streamId);

            // Assert
            _chunkMessageSender.Received(1).Send(
                clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == RtmpConstants.DataMessageChunkStreamId),
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
            var clientContexts = new List<IRtmpClientContext>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var streamMetaData = _fixture.Create<Dictionary<string, object>>();
            var timestamp = _fixture.Create<uint>();
            var streamId = _fixture.Create<uint>();

            clientContexts.Add(Substitute.For<IRtmpClientContext>());
            clientContexts.Add(Substitute.For<IRtmpClientContext>());

            publishStreamContext.StreamMetaData.Returns(streamMetaData);

            using var metaDataBuffer = new DataBuffer();
            _chunkMessageSender.When(
                x => x.Send(
                    clientContexts,
                    Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == RtmpConstants.DataMessageChunkStreamId),
                    Arg.Is<RtmpChunkMessageHeaderType0>(x => x.MessageTypeId == RtmpMessageType.DataMessageAmf0),
                    Arg.Any<Action<IDataBuffer>>()
                )
            ).Do(x => x.Arg<Action<IDataBuffer>>().Invoke(metaDataBuffer));

            // Act
            _sut.SendCachedStreamMetaDataMessage(clientContexts, publishStreamContext, timestamp, streamId);

            // Assert
            _chunkMessageSender.Received(1).Send(
                clientContexts,
                Arg.Is<RtmpChunkBasicHeader>(x => x.ChunkType == 0 && x.ChunkStreamId == RtmpConstants.DataMessageChunkStreamId),
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
            var clientContext = Substitute.For<IRtmpClientContext>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var streamId = _fixture.Create<uint>();
            var outChunkSize = _fixture.Create<uint>();

            clientContext.OutChunkSize.Returns(outChunkSize);
            clientContext.UpdateTimestamp(Arg.Any<uint>(), Arg.Any<MediaType>()).Returns(true);

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

            publishStreamContext.GroupOfPicturesCache.Get().Returns(groupOfPictures);

            using var payloadsBuffer = new DataBuffer();
            _chunkMessageSender.When(
                x => x.Send(
                    clientContext,
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
            _sut.SendCachedGroupOfPictures(clientContext, publishStreamContext, streamId);

            // Assert
            _chunkMessageSender.Received(groupOfPictures.Count).Send(
                clientContext,
                Arg.Any<RtmpChunkBasicHeader>(),
                Arg.Any<RtmpChunkMessageHeaderType0>(),
                Arg.Any<Action<IDataBuffer>>());

            payloadsBuffer.UnderlyingBuffer.Take(payloadsBuffer.Size).Should().BeEquivalentTo(expectedPayloadsBufffer);
        }
    }
}
