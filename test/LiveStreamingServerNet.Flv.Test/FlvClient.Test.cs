using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Flv.Internal;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace LiveStreamingServerNet.Flv.Test
{
    public class FlvClientTest
    {
        private readonly IFixture _fixture;
        private readonly string _clientId;
        private readonly string _streamPath;
        private readonly CancellationTokenSource _stoppingCts;
        private readonly CancellationToken _stoppingToken;
        private readonly IFlvMediaTagBroadcasterService _mediaTagBroadcaster;
        private readonly IFlvWriter _flvWriter;
        private readonly ILogger<FlvClient> _logger;
        private readonly IFlvClient _sut;

        public FlvClientTest()
        {
            _fixture = new Fixture();
            _clientId = _fixture.Create<string>();
            _streamPath = _fixture.Create<string>();
            _stoppingCts = new CancellationTokenSource();
            _stoppingToken = _stoppingCts.Token;

            _mediaTagBroadcaster = Substitute.For<IFlvMediaTagBroadcasterService>();
            _flvWriter = Substitute.For<IFlvWriter>();
            _logger = Substitute.For<ILogger<FlvClient>>();
            _sut = new FlvClient(_clientId, _streamPath, _mediaTagBroadcaster, _flvWriter, _logger, _stoppingToken);
        }

        [Fact]
        public void Constructor_Should_RegisterClient_WithMediaTagManager()
        {
            // Assert
            _mediaTagBroadcaster.Received(1).RegisterClient(_sut);

            _sut.UntilInitializationComplete().IsCompleted.Should().BeFalse();
            _sut.UntilComplete().IsCompleted.Should().BeFalse();
        }

        [Fact]
        public void CompleteInitialization_Should_MarkInitializationComplete()
        {
            // Act
            _sut.CompleteInitialization();

            // Assert
            _sut.UntilInitializationComplete().IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void Stop_Should_CancelInitializationAndMarkComplete()
        {
            // Act
            _sut.Stop();

            // Assert
            _sut.UntilInitializationComplete().IsCanceled.Should().BeTrue();
            _sut.UntilComplete().IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void CancellingStoppingToken_Should_CancelInitializationAndMarkComplete()
        {
            // Act
            _stoppingCts.Cancel();

            // Assert
            _sut.UntilInitializationComplete().IsCanceled.Should().BeTrue();
            _sut.UntilComplete().IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task DisposeAsync_Should_UnregisterClient_FromMediaTagManager()
        {
            // Act
            await _sut.DisposeAsync();

            // Assert
            _mediaTagBroadcaster.Received(1).UnregisterClient(_sut);
        }

        [Fact]
        public async Task DisposeAsync_Should_CancelInitializationAndMarkComplete()
        {
            // Act
            await _sut.DisposeAsync();

            // Assert
            _sut.UntilInitializationComplete().IsCanceled.Should().BeTrue();
            _sut.UntilComplete().IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task DisposeAsync_Should_DisposeFlvWriter()
        {
            // Act
            await _sut.DisposeAsync();

            // Assert
            await _flvWriter.Received(1).DisposeAsync();
        }


        [Fact]
        public async Task WriteHeaderAsync_Should_WriteHeader()
        {
            // Arrange
            var allowAudioTags = _fixture.Create<bool>();
            var allowVideoTags = _fixture.Create<bool>();

            // Act
            await _sut.WriteHeaderAsync(allowAudioTags, allowVideoTags, default);

            // Assert
            await _flvWriter.Received(1).WriteHeaderAsync(allowAudioTags, allowVideoTags, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WriteHeaderAsync_Should_StopClient_When_ExceptionThrown()
        {
            // Arrange
            var allowAudioTags = _fixture.Create<bool>();
            var allowVideoTags = _fixture.Create<bool>();

            _flvWriter.WriteHeaderAsync(allowAudioTags, allowVideoTags, Arg.Any<CancellationToken>())
                      .Throws(new Exception());

            // Act
            await _sut.WriteHeaderAsync(allowAudioTags, allowVideoTags, default);

            // Assert
            _sut.UntilComplete().IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task WriteTagAsync_Should_WriteTag()
        {
            // Arrange
            var tagType = _fixture.Create<FlvTagType>();
            var timestamp = _fixture.Create<uint>();
            var payloadBuffer = Substitute.For<Action<IDataBuffer>>();

            // Act
            await _sut.WriteTagAsync(tagType, timestamp, payloadBuffer, default);

            // Assert
            await _flvWriter.Received(1).WriteTagAsync(tagType, timestamp, payloadBuffer, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WriteTagAsync_Should_Stop_When_ExceptionThrown()
        {
            // Arrange
            var tagType = _fixture.Create<FlvTagType>();
            var timestamp = _fixture.Create<uint>();
            var payloadBuffer = Substitute.For<Action<IDataBuffer>>();
            _flvWriter.WriteTagAsync(tagType, timestamp, payloadBuffer, Arg.Any<CancellationToken>()).Throws(new Exception());

            // Act
            await _sut.WriteTagAsync(tagType, timestamp, payloadBuffer, default);

            // Assert
            _sut.UntilComplete().IsCompleted.Should().BeTrue();
        }
    }
}
