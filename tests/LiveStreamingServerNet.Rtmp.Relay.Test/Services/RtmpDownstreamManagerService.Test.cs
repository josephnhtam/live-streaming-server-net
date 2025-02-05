using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Streams.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Relay.Test.Services
{
    public class RtmpDownstreamManagerServiceTest
    {
        private readonly IFixture _fixture;
        private readonly ServiceProvider _services;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpDownstreamProcessFactory _downstreamProcessFactory;
        private readonly IRtmpDownstreamProcess _downstreamProcess;
        private readonly RtmpDownstreamConfiguration _config;
        private readonly RtmpDownstreamManagerService _sut;
        private Task? _downstreamTask;

        public RtmpDownstreamManagerServiceTest()
        {
            _fixture = new Fixture();
            _services = new ServiceCollection().BuildServiceProvider();
            _streamManager = Substitute.For<IRtmpStreamManagerService>();
            _downstreamProcess = Substitute.For<IRtmpDownstreamProcess>();
            _downstreamProcessFactory = Substitute.For<IRtmpDownstreamProcessFactory>();

            _downstreamProcessFactory
                .Create(Arg.Any<string>())
                .Returns(_downstreamProcess);

            _downstreamProcess.RunAsync(Arg.Any<CancellationToken>())
                .Returns(info =>
                {
                    var tcs = new TaskCompletionSource();
                    info.Arg<CancellationToken>().Register(() => tcs.SetCanceled());

                    _downstreamTask = tcs.Task;
                    return _downstreamTask;
                });

            _config = new RtmpDownstreamConfiguration
            {
                Enabled = true
            };

            _sut = new RtmpDownstreamManagerService(
                _services,
                _downstreamProcessFactory,
                _streamManager,
                Options.Create(_config));
        }

        [Fact]
        public async Task OnRtmpStreamSubscribedAsync_Should_CreateDownStreamProcess_WhenDownStreamIsBeingSubscribed()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = _fixture.Create<uint>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            _streamManager.IsStreamBeingSubscribed(streamPath).Returns(true);

            // Act
            await _sut.OnRtmpStreamSubscribedAsync(context, clientId, streamPath, streamArguments);

            // Assert
            _downstreamProcessFactory.Received().Create(streamPath);
        }

        [Fact]
        public async Task OnRtmpStreamSubscribedAsync_Should_CreateDownStreamProcess_WhenStreamIsBeingRequested()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = _fixture.Create<uint>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            using var subscriber = await _sut.RequestDownstreamAsync(streamPath);

            // Act
            await _sut.OnRtmpStreamSubscribedAsync(context, clientId, streamPath, streamArguments);

            // Assert
            _downstreamProcessFactory.Received().Create(streamPath);
        }

        [Fact]
        public async Task OnRtmpStreamSubscribedAsync_Should_CreateDownStreamProcess_WhenStreamIsNotBeingRequested()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = _fixture.Create<uint>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            // Act
            await _sut.OnRtmpStreamSubscribedAsync(context, clientId, streamPath, streamArguments);

            // Assert
            _downstreamProcessFactory.DidNotReceive().Create(streamPath);
        }

        [Fact]
        public async Task IsDownstreamRequested_Should_BeTrue_WhenStreamIsBeingRequested()
        {
            // Arrange
            var context = Substitute.For<IEventContext>();
            var clientId = _fixture.Create<uint>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            // Act
            using (var subscriber = await _sut.RequestDownstreamAsync(streamPath))
            {
                // Assert
                _sut.IsDownstreamRequested(streamPath).Should().BeTrue();
            }

            // Assert
            _sut.IsDownstreamRequested(streamPath).Should().BeFalse();
        }
    }
}
