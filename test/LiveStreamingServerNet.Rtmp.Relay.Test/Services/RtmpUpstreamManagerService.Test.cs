using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Streams.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Relay.Test.Services
{
    public class RtmpUpstreamManagerServiceTest
    {
        private readonly IFixture _fixture;
        private readonly ServiceProvider _services;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpUpstreamProcessFactory _upstreamProcessFactory;
        private readonly IRtmpUpstreamProcess _upstreamProcess;
        private readonly RtmpUpstreamConfiguration _config;
        private readonly RtmpUpstreamManagerService _sut;
        private Task? _upstreamTask;

        public RtmpUpstreamManagerServiceTest()
        {
            _fixture = new Fixture();
            _services = new ServiceCollection().BuildServiceProvider();
            _streamManager = Substitute.For<IRtmpStreamManagerService>();
            _upstreamProcess = Substitute.For<IRtmpUpstreamProcess>();
            _upstreamProcessFactory = Substitute.For<IRtmpUpstreamProcessFactory>();

            _upstreamProcessFactory
                .Create(Arg.Any<IRtmpPublishStreamContext>())
                .Returns(_upstreamProcess);

            _upstreamProcess.RunAsync(Arg.Any<CancellationToken>())
                .Returns(info =>
                {
                    var tcs = new TaskCompletionSource();
                    info.Arg<CancellationToken>().Register(() => tcs.SetCanceled());

                    _upstreamTask = tcs.Task;
                    return _upstreamTask;
                });

            _config = new RtmpUpstreamConfiguration
            {
                Enabled = true
            };

            _sut = new RtmpUpstreamManagerService(
                _services,
                _upstreamProcessFactory,
                _streamManager,
                Options.Create(_config));
        }

        [Fact]
        public async Task OnRtmpStreamPublishedAsync_Should_CreateUpStreamProcess_WhenClientIdIsNotZero()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = 1u;
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            // Act
            await _sut.OnRtmpStreamPublishedAsync(context, clientId, streamPath, streamArguments);

            // Assert
            _upstreamProcessFactory.Received().Create(Arg.Any<IRtmpPublishStreamContext>());
        }

        [Fact]
        public async Task OnRtmpStreamPublishedAsync_Should_DoNothing_WhenClientIdIsZero()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = 0u;
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            // Act
            await _sut.OnRtmpStreamPublishedAsync(context, clientId, streamPath, streamArguments);

            // Assert
            _upstreamProcessFactory.DidNotReceive().Create(Arg.Any<IRtmpPublishStreamContext>());
        }

        [Fact]
        public async Task OnReceiveMediaMessageAsync_Should_ForwardMediaDataToUpstreamProcess()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = 1u;
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var mediaType = _fixture.Create<MediaType>();
            var buffer = new RentedBuffer(1024);
            var timestamp = _fixture.Create<uint>();
            var isSkippable = _fixture.Create<bool>();

            await _sut.OnRtmpStreamPublishedAsync(context, clientId, streamPath, streamArguments);

            // Act
            await _sut.OnReceiveMediaMessageAsync(clientId, streamPath, mediaType, buffer, timestamp, isSkippable);

            // Assert
            _upstreamProcess.Received().OnReceiveMediaData(mediaType, buffer, timestamp, isSkippable);
        }

        [Fact]
        public async Task OnRtmpStreamMetaDataReceivedAsync_Should_ForwardMetaDataToUpstreamProcess()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = 1u;
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var metaData = _fixture.Create<Dictionary<string, object>>();

            await _sut.OnRtmpStreamPublishedAsync(context, clientId, streamPath, streamArguments);

            // Act
            await _sut.OnRtmpStreamMetaDataReceivedAsync(context, clientId, streamPath, metaData);

            // Assert
            _upstreamProcess.Received().OnReceiveMetaData(metaData);
        }

        [Fact]
        public async Task FilterMediaMessage_Should_ReturnTrue_WhenStreamPathIsRegistered()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = 1u;
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            await _sut.OnRtmpStreamPublishedAsync(context, clientId, streamPath, streamArguments);

            // Act
            var result = _sut.FilterMediaMessage(streamPath, _fixture.Create<MediaType>(), _fixture.Create<uint>(), _fixture.Create<bool>());

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task FilterMediaMessage_Should_ReturnFalse_WhenStreamPathIsNotRegistered()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = 1u;
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            await _sut.OnRtmpStreamPublishedAsync(context, clientId, streamPath, streamArguments);

            // Act
            var result = _sut.FilterMediaMessage(
                $"another_{streamPath}", _fixture.Create<MediaType>(), _fixture.Create<uint>(), _fixture.Create<bool>());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task OnRtmpStreamUnpublishedAsync_Should_RemoveUpstreamProcess()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = 1u;
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            _streamManager.IsStreamPublishing(streamPath).Returns(false);
            await _sut.OnRtmpStreamPublishedAsync(context, clientId, streamPath, streamArguments);
            _streamManager.GetPublishStreamContext(streamPath).Returns((IRtmpPublishStreamContext?)null);

            // Act
            await _sut.OnRtmpStreamUnpublishedAsync(context, clientId, streamPath);

            // Assert
            _upstreamTask!.IsCanceled.Should().BeTrue();
        }
    }
}
