using AutoFixture;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test.Services
{
    public class FlvMediaTagCacherServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IFlvClient _client;
        private readonly IFlvStreamContext _streamContext;
        private readonly IFlvMediaTagSenderService _mediaTagSender;
        private readonly FlvMediaTagCacherService _cacherService;

        public FlvMediaTagCacherServiceTest()
        {
            _fixture = new Fixture();
            _client = Substitute.For<IFlvClient>();
            _streamContext = Substitute.For<IFlvStreamContext>();
            _mediaTagSender = Substitute.For<IFlvMediaTagSenderService>();
            _cacherService = new FlvMediaTagCacherService(_mediaTagSender);
        }

        [Fact]
        public async Task CacheSequenceHeaderAsync_Should_SetVideoSequenceHeader_WhenMediaTypeIsVideo()
        {
            // Arrange
            var sequenceHeader = _fixture.Create<byte[]>();

            // Act
            await _cacherService.CacheSequenceHeaderAsync(_streamContext, MediaType.Video, sequenceHeader);

            // Assert
            _streamContext.Received(1).VideoSequenceHeader = Arg.Is<byte[]>(x => x.SequenceEqual(sequenceHeader));
        }

        [Fact]
        public async Task CacheSequenceHeaderAsync_Should_SetAudioSequenceHeader_WhenMediaTypeIsAudio()
        {
            // Arrange
            var sequenceHeader = _fixture.Create<byte[]>();

            // Act
            await _cacherService.CacheSequenceHeaderAsync(_streamContext, MediaType.Audio, sequenceHeader);

            // Assert
            _streamContext.Received(1).AudioSequenceHeader = Arg.Is<byte[]>(x => x.SequenceEqual(sequenceHeader));
        }

        [Fact]
        public async Task CachePictureAsync_Should_AddPictureToGroupOfPicturesCache()
        {
            // Arrange
            var mediaType = _fixture.Create<MediaType>();
            var rentedBuffer = Substitute.For<IRentedBuffer>();
            var timestamp = _fixture.Create<uint>();

            // Act
            await _cacherService.CachePictureAsync(_streamContext, mediaType, rentedBuffer, timestamp);

            // Assert
            Received.InOrder(() =>
            {
                _streamContext.Received(1).GroupOfPicturesCache.Add(Arg.Is<PictureCacheInfo>(x =>
                    x.Type == mediaType && x.Timestamp == timestamp), rentedBuffer.Buffer, 0, rentedBuffer.Size);
            });
        }

        [Fact]
        public async Task ClearGroupOfPicturesCacheAsync_Should_ClearGroupOfPicturesCache()
        {
            // Act
            await _cacherService.ClearGroupOfPicturesCacheAsync(_streamContext);

            // Assert
            _streamContext.Received(1).GroupOfPicturesCache.Clear();
        }

        [Fact]
        public async Task SendCachedGroupOfPicturesTagsAsync_Should_SendMediaTagsForCachedPictures()
        {
            // Arrange
            var pictures = new List<PictureCache>()
            {
                new PictureCache(_fixture.Create<MediaType>(), 1, Substitute.For<IRentedBuffer>()),
                new PictureCache(_fixture.Create<MediaType>(), 2, Substitute.For<IRentedBuffer>()),
                new PictureCache(_fixture.Create<MediaType>(), 3, Substitute.For<IRentedBuffer>())
            };
            _streamContext.GroupOfPicturesCache.Get().Returns(pictures);

            // Act
            await _cacherService.SendCachedGroupOfPicturesTagsAsync(_client, _streamContext, default);

            // Assert
            foreach (var picture in pictures)
            {
                await _mediaTagSender.Received(1).SendMediaTagAsync(
                    _client,
                    picture.Type,
                    picture.Payload.Buffer,
                    picture.Payload.Size,
                    picture.Timestamp,
                    Arg.Any<CancellationToken>());

                picture.Payload.Received(1).Unclaim();
            }
        }

        [Fact]
        public async Task SendCachedHeaderTagsAsync_Should_SendMediaTagsForCachedHeader()
        {
            // Arrange
            var timestamp = _fixture.Create<uint>();
            var audioSequenceHeader = _fixture.Create<byte[]>();
            var videoSequenceHeader = _fixture.Create<byte[]>();

            _streamContext.AudioSequenceHeader.Returns(audioSequenceHeader);
            _streamContext.VideoSequenceHeader.Returns(videoSequenceHeader);

            // Act
            await _cacherService.SendCachedHeaderTagsAsync(_client, _streamContext, timestamp, CancellationToken.None);

            // Assert
            await _mediaTagSender.Received(1).SendMediaTagAsync(
                _client,
                MediaType.Audio,
                audioSequenceHeader,
                audioSequenceHeader.Length,
                timestamp,
                CancellationToken.None);

            await _mediaTagSender.Received(1).SendMediaTagAsync(
                _client,
                MediaType.Video,
                videoSequenceHeader,
                videoSequenceHeader.Length,
                timestamp,
                CancellationToken.None);
        }
    }
}
