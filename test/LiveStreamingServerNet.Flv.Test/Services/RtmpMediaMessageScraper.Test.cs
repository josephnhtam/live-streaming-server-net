using AutoFixture;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test.Services
{
    public class RtmpMediaMessageScraperTest
    {
        private readonly IFixture _fixture;
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IFlvMediaTagBroadcasterService _mediaTagBroadcaster;
        private readonly RtmpMediaMessageScraper _sut;

        public RtmpMediaMessageScraperTest()
        {
            _fixture = new Fixture();
            _streamManager = Substitute.For<IFlvStreamManagerService>();
            _mediaTagBroadcaster = Substitute.For<IFlvMediaTagBroadcasterService>();

            _sut = new RtmpMediaMessageScraper(
                _streamManager,
                _mediaTagBroadcaster);
        }

        [Fact]
        public async Task OnReceiveMediaMessage_Should_BroadcastMediaTag()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();
            var isSkippable = _fixture.Create<bool>();
            var rentedBuffer = Substitute.For<IRentedBuffer>();

            var streamContext = Substitute.For<IFlvStreamContext>();
            _streamManager.GetFlvStreamContext(streamPath).Returns(streamContext);

            var subscribers = new List<IFlvClient>() { Substitute.For<IFlvClient>() };
            _streamManager.GetSubscribers(streamPath).Returns(subscribers);

            // Act
            await _sut.OnReceiveMediaMessage(streamPath, mediaType, rentedBuffer, timestamp, isSkippable);

            // Assert
            await _mediaTagBroadcaster.BroadcastMediaTagAsync(streamContext, subscribers, mediaType, timestamp, isSkippable, rentedBuffer);
        }
    }
}
