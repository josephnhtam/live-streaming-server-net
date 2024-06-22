using AutoFixture;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test.Services
{
    public class RtmpMediaCacheScraperTest
    {
        private readonly IFixture _fixture;
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IFlvMediaTagBroadcasterService _mediaTagBroadcaster;
        private readonly IFlvMediaTagCacherService _mediaTagCacher;
        private readonly RtmpMediaCacheScraper _sut;

        public RtmpMediaCacheScraperTest()
        {
            _fixture = new Fixture();
            _streamManager = Substitute.For<IFlvStreamManagerService>();
            _mediaTagBroadcaster = Substitute.For<IFlvMediaTagBroadcasterService>();
            _mediaTagCacher = Substitute.For<IFlvMediaTagCacherService>();

            _sut = new RtmpMediaCacheScraper(
                _streamManager,
                _mediaTagBroadcaster,
                _mediaTagCacher);
        }

        [Fact]
        public async Task OnCachePicture_Should_CachePicture()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();
            var rentedBuffer = Substitute.For<IRentedBuffer>();

            var streamContext = Substitute.For<IFlvStreamContext>();
            _streamManager.GetFlvStreamContext(streamPath).Returns(streamContext);

            // Act
            await _sut.OnCachePicture(streamPath, mediaType, rentedBuffer, timestamp);

            // Assert
            await _mediaTagCacher.Received(1).CachePictureAsync(streamContext, mediaType, rentedBuffer, timestamp);
        }

        [Fact]
        public async Task OnClearGroupOfPicturesCache_Should_ClearGroupOfPicturesCache()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();

            var streamContext = Substitute.For<IFlvStreamContext>();
            _streamManager.GetFlvStreamContext(streamPath).Returns(streamContext);

            // Act
            await _sut.OnClearGroupOfPicturesCache(streamPath);

            // Assert
            await _mediaTagCacher.Received(1).ClearGroupOfPicturesCacheAsync(streamContext);
        }

        [Fact]
        public async Task OnCacheSequenceHeader_Should_BroadcastSequenceHeader()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var mediaType = _fixture.Create<MediaType>();
            var sequenceHeader = _fixture.CreateMany<byte>().ToArray();

            var streamContext = Substitute.For<IFlvStreamContext>();
            _streamManager.GetFlvStreamContext(streamPath).Returns(streamContext);

            var subscribers = new List<IFlvClient>() { Substitute.For<IFlvClient>() };
            _streamManager.GetSubscribers(streamPath).Returns(subscribers);

            // Act
            await _sut.OnCacheSequenceHeader(streamPath, mediaType, sequenceHeader);

            // Assert
            await _mediaTagBroadcaster.BroadcastMediaTagAsync(streamContext, subscribers, mediaType, 0, false,
                Arg.Is<IRentedBuffer>(x => x.Buffer.SequenceEqual(sequenceHeader)));
        }
    }
}
