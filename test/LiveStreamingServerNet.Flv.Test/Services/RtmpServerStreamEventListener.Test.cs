using AutoFixture;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Flv.Test.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test.Services
{
    public class RtmpServerStreamEventListenerTest
    {
        private readonly IFixture _fixture;
        private readonly IEventContext _context;
        private readonly IFlvStreamManagerService _streamManager;
        private readonly RtmpServerStreamEventListener _sut;

        public RtmpServerStreamEventListenerTest()
        {
            _fixture = new Fixture();
            _context = Substitute.For<IEventContext>();
            _streamManager = Substitute.For<IFlvStreamManagerService>();
            _sut = new RtmpServerStreamEventListener(_streamManager);
        }

        [Fact]
        public async Task OnRtmpStreamPublishedAsync_Should_StartPublishingStream()
        {
            // Arrange
            var clientId = _fixture.Create<uint>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            // Act
            await _sut.OnRtmpStreamPublishedAsync(_context, clientId, streamPath, streamArguments);

            // Assert
            _streamManager.Received(1).StartPublishingStream(Arg.Is<IFlvStreamContext>(x =>
                x.StreamPath == streamPath &&
                x.StreamArguments.Match(streamArguments)));
        }

        [Fact]
        public async Task OnRtmpStreamUnpublishedAsync_Should_StopPublishingStreamAndStopSubscribers()
        {
            // Arrange
            var clientId = _fixture.Create<uint>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var client1 = Substitute.For<IFlvClient>();
            var client2 = Substitute.For<IFlvClient>();
            var subscribers = new List<IFlvClient>() { client1, client2 };

            _streamManager.StopPublishingStream(streamPath, out _).Returns(x =>
            {
                x[1] = subscribers;
                return true;
            });

            // Act
            await _sut.OnRtmpStreamUnpublishedAsync(_context, clientId, streamPath);

            // Assert
            _streamManager.Received(1).StopPublishingStream(streamPath, out _);
            client1.Received(1).Stop();
            client2.Received(1).Stop();
        }

        [Fact]
        public async Task OnRtmpStreamMetaDataReceivedAsync_Should_UpdateStreamMetaData()
        {
            // Arrange
            var clientId = _fixture.Create<uint>();
            var streamPath = _fixture.Create<string>();
            var metaData = _fixture.Create<Dictionary<string, object>>();

            var streamContext = Substitute.For<IFlvStreamContext>();
            _streamManager.GetFlvStreamContext(streamPath).Returns(streamContext);

            // Act
            await _sut.OnRtmpStreamMetaDataReceivedAsync(_context, clientId, streamPath, metaData);

            // Assert
            streamContext.Received(1).StreamMetaData = Arg.Is<IReadOnlyDictionary<string, object>>(
                x => x.Match(metaData)
            );
        }
    }
}
