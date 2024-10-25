using AutoFixture;
using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test.Services
{
    public class FlvClientHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IFlvMediaTagCacherService _mediaTagCacher;
        private readonly FlvConfiguration _config;
        private readonly ILogger<FlvClientHandler> _logger;
        private readonly string _streamPath;
        private readonly IFlvClient _client;
        private readonly IFlvStreamContext _streamContext;
        private readonly FlvClientHandler _sut;

        public FlvClientHandlerTest()
        {
            _fixture = new Fixture();
            _streamManager = Substitute.For<IFlvStreamManagerService>();
            _mediaTagCacher = Substitute.For<IFlvMediaTagCacherService>();
            _config = new FlvConfiguration();
            _logger = Substitute.For<ILogger<FlvClientHandler>>();

            _streamPath = _fixture.Create<string>();
            _client = Substitute.For<IFlvClient>();
            _client.StreamPath.Returns(_streamPath);

            _streamContext = Substitute.For<IFlvStreamContext>();
            _streamManager.GetFlvStreamContext(_client.StreamPath).Returns(_streamContext);

            _sut = new FlvClientHandler(_streamManager, _mediaTagCacher, Options.Create(_config), _logger);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task RunClientAsync_Should_SendFlvHeaderAndCachedFlvTags(bool hasAudio, bool hasVideo)
        {
            // Arrange
            var autioSequenceHeader = hasAudio ? _fixture.Create<byte[]>() : null;
            var videoSequenceHeader = hasVideo ? _fixture.Create<byte[]>() : null;

            _streamContext.AudioSequenceHeader.Returns(autioSequenceHeader);
            _streamContext.VideoSequenceHeader.Returns(videoSequenceHeader);

            // Act
            await _sut.RunClientAsync(_client, default);

            // Assert
            Received.InOrder(() =>
            {
                _client.Received(1).WriteHeaderAsync(hasAudio, hasVideo, Arg.Any<CancellationToken>());
                _mediaTagCacher.Received().SendCachedHeaderTagsAsync(_client, _streamContext, 0, Arg.Any<CancellationToken>());
                _mediaTagCacher.Received().SendCachedGroupOfPicturesTagsAsync(_client, _streamContext, Arg.Any<CancellationToken>());

                _client.Received().CompleteInitialization();
            });
        }

        [Fact]
        public async Task RunClientAsync_Should_Stop_After_ClientIsComplete()
        {
            // Arrange
            _client.UntilCompleteAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            // Act
            await _sut.RunClientAsync(_client, default);

            // Assert
            _streamManager.Received().StopSubscribingStream(_client);
        }
    }
}
