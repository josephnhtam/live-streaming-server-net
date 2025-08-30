using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Test.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.Services
{
    public class RtmpStreamManagerServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger<RtmpClientSessionContext> _logger;
        private readonly IRtmpStreamManagerService _sut;

        public RtmpStreamManagerServiceTest()
        {
            _fixture = new Fixture();
            _commandMessageSender = Substitute.For<IRtmpCommandMessageSenderService>();
            _userControlMessageSender = Substitute.For<IRtmpUserControlMessageSenderService>();
            _eventDispatcher = Substitute.For<IRtmpServerStreamEventDispatcher>();
            _config = new RtmpServerConfiguration();
            _logger = Substitute.For<ILogger<RtmpClientSessionContext>>();
            _sut = new RtmpStreamManagerService(_commandMessageSender, _userControlMessageSender, _eventDispatcher, Options.Create(_config));
        }

        [Fact]
        public async Task GetPublishStreamContext_Should_ReturnCorrectStreamContext()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null, _config, _logger);
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var streamContext = clientContext.CreateStreamContext();
            await _sut.StartPublishingAsync(streamContext, streamPath, streamArguments);

            // Act
            var result = _sut.GetPublishStreamContext(streamPath);

            // Assert
            result.Should().Be(streamContext.PublishContext);
        }

        [Fact]
        public async Task StartPublishingAsync_Should_AddPublisherAndReturnSucceeded()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            streamContext.ClientContext.Returns(clientContext);
            streamContext.PublishContext.Returns((IRtmpPublishStreamContext?)null);
            streamContext.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);

            // Act
            var result = await _sut.StartPublishingAsync(streamContext, streamPath, streamArguments);

            // Assert
            result.Result.Should().Be(PublishingStreamResult.Succeeded);
            streamContext.Received(1).CreatePublishContext(streamPath, streamArguments);
            _sut.IsStreamPublishing(streamPath).Should().BeTrue();
        }

        [Fact]
        public async Task StartPublishingAsync_Should_ReturnAlreadySubscribing_When_PublisherIsAlreadySubscribing()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            streamContext.ClientContext.Returns(clientContext);
            streamContext.PublishContext.Returns((IRtmpPublishStreamContext?)null);
            streamContext.SubscribeContext.Returns(subscribeStreamContext);
            subscribeStreamContext.StreamContext.Returns(streamContext);

            // Act
            var result = await _sut.StartPublishingAsync(streamContext, streamPath, streamArguments);

            // Assert
            result.Result.Should().Be(PublishingStreamResult.AlreadySubscribing);
        }

        [Fact]
        public async Task StartPublishingAsync_Should_ReturnAlreadyPublishing_When_PublisherIsAlreadyPublishing()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            streamContext.ClientContext.Returns(clientContext);
            streamContext.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);
            streamContext.PublishContext.Returns(publishStreamContext);
            publishStreamContext.StreamContext.Returns(streamContext);

            // Act
            var result = await _sut.StartPublishingAsync(streamContext, streamPath, streamArguments);

            // Assert
            result.Result.Should().Be(PublishingStreamResult.AlreadyPublishing);
        }

        [Fact]
        public async Task StartPublishingAsync_Should_ReturnAlreadyExists_When_StreamPathAlreadyExists()
        {
            // Arrange
            var sessionHandle1 = Substitute.For<ISessionHandle>();
            var clientContext1 = new RtmpClientSessionContext(sessionHandle1, null, _config, _logger);
            var streamContext1 = clientContext1.CreateStreamContext();

            var sessionHandle2 = Substitute.For<ISessionHandle>();
            var clientContext2 = new RtmpClientSessionContext(sessionHandle2, null, _config, _logger);
            var streamContext2 = clientContext2.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            await _sut.StartPublishingAsync(streamContext1, streamPath, streamArguments);

            // Act
            var result = await _sut.StartPublishingAsync(streamContext2, streamPath, streamArguments);

            // Assert
            result.Result.Should().Be(PublishingStreamResult.AlreadyExists);
        }

        [Fact]
        public async Task StopPublishingAsync_Should_RemovePublisherAndReturnTrue()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null, _config, _logger);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            await _sut.StartPublishingAsync(streamContext, streamPath, streamArguments);

            // Act
            var result = await _sut.StopPublishingAsync(streamContext.PublishContext!);

            // Assert
            result.Result.Should().BeTrue();
            streamContext.PublishContext.Should().BeNull();
            _sut.IsStreamPublishing(streamPath).Should().BeFalse();
        }

        [Fact]
        public async Task StopPublishingAsync_Should_ReturnFalse_When_PublisherDoesNotExist()
        {
            // Arrange
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();

            // Act
            var result = await _sut.StopPublishingAsync(publishStreamContext);

            // Assert
            result.Result.Should().BeFalse();
        }

        [Fact]
        public async Task IsStreamPathPublishing_Should_ReturnTrue_When_StreamPathIsPublishing()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null, _config, _logger);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            await _sut.StartPublishingAsync(streamContext, streamPath, streamArguments);

            // Act
            var result = _sut.IsStreamPublishing(streamPath);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsStreamPathPublishing_Should_ReturnFalse_When_StreamPathIsNotPublishing()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();

            // Act
            var result = _sut.IsStreamPublishing(streamPath);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task StartSubscribingAsync_Should_AddSubscriberAndReturnSucceeded()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null, _config, _logger);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            // Act
            var result = await _sut.StartSubscribingAsync(streamContext, streamPath, streamArguments);

            // Assert
            result.Result.Should().Be(SubscribingStreamResult.Succeeded);
            _sut.GetSubscribeStreamContexts(streamPath).Should().Contain(streamContext.SubscribeContext!);
        }

        [Fact]
        public async Task StartSubscribingAsync_Should_ReturnAlreadyPublishing_When_SubscriberIsAlreadyPublishing()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null, _config, _logger);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            await _sut.StartPublishingAsync(streamContext, streamPath, streamArguments);

            // Act
            var result = await _sut.StartSubscribingAsync(streamContext, streamPath, streamArguments);

            // Assert
            result.Result.Should().Be(SubscribingStreamResult.AlreadyPublishing);
        }

        [Fact]
        public async Task StartSubscribingAsync_Should_ReturnAlreadySubscribing_When_SubscriberIsAlreadySubscribing()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null, _config, _logger);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            await _sut.StartSubscribingAsync(streamContext, streamPath, streamArguments);

            // Act
            var result = await _sut.StartSubscribingAsync(streamContext, streamPath, streamArguments);

            // Assert
            result.Result.Should().Be(SubscribingStreamResult.AlreadySubscribing);
        }

        [Fact]
        public async Task StopSubscribingAsync_Should_RemoveSubscriberAndReturnTrue()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null, _config, _logger);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            await _sut.StartSubscribingAsync(streamContext, streamPath, streamArguments);

            var subscribeStreamContext = streamContext.SubscribeContext!;

            // Act
            var result = await _sut.StopSubscribingAsync(subscribeStreamContext);

            // Assert
            result.Should().BeTrue();
            _sut.GetSubscribeStreamContexts(streamPath).Should().NotContain(subscribeStreamContext);
        }

        [Fact]
        public async Task StopSubscribingAsync_Should_ReturnFalse_When_SubscriberDoesNotExist()
        {
            // Arrange
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();

            // Act
            var result = await _sut.StopSubscribingAsync(subscribeStreamContext);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetSubscribeStreamContexts_Should_ReturnCorrectSubscriberStreamContexts()
        {
            // Arrange
            var sessionHandle1 = Substitute.For<ISessionHandle>();
            var clientContext1 = new RtmpClientSessionContext(sessionHandle1, null, _config, _logger);
            var streamContext1 = clientContext1.CreateStreamContext();

            var sessionHandle2 = Substitute.For<ISessionHandle>();
            var clientContext2 = new RtmpClientSessionContext(sessionHandle2, null, _config, _logger);
            var streamContext2 = clientContext2.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            await _sut.StartSubscribingAsync(streamContext1, streamPath, streamArguments);
            await _sut.StartSubscribingAsync(streamContext2, streamPath, streamArguments);

            // Act
            var result = _sut.GetSubscribeStreamContexts(streamPath);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(streamContext1.SubscribeContext!);
            result.Should().Contain(streamContext2.SubscribeContext!);
        }

        [Fact]
        public async Task StartPublishingAsync_Should_ReturnCorrectSubscribers()
        {
            // Arrange
            var sessionHandle1 = Substitute.For<ISessionHandle>();
            var clientContext1 = new RtmpClientSessionContext(sessionHandle1, null, _config, _logger);
            var streamContext1 = clientContext1.CreateStreamContext();

            var sessionHandle2 = Substitute.For<ISessionHandle>();
            var clientContext2 = new RtmpClientSessionContext(sessionHandle2, null, _config, _logger);
            var streamContext2 = clientContext2.CreateStreamContext();

            var sessionHandle3 = Substitute.For<ISessionHandle>();
            var clientContext3 = new RtmpClientSessionContext(sessionHandle3, null, _config, _logger);
            var streamContext3 = clientContext2.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            await _sut.StartSubscribingAsync(streamContext1, streamPath, streamArguments);
            await _sut.StartSubscribingAsync(streamContext2, streamPath, streamArguments);

            // Act
            var (result, existingSubscribers) = await _sut.StartPublishingAsync(streamContext3, streamPath, streamArguments);

            // Assert
            result.Should().Be(PublishingStreamResult.Succeeded);
            _sut.IsStreamPublishing(streamPath).Should().BeTrue();
            existingSubscribers.Should().HaveCount(2);
            existingSubscribers.Should().Contain(streamContext1.SubscribeContext!);
            existingSubscribers.Should().Contain(streamContext2.SubscribeContext!);
        }

        [Fact]
        public async Task StartPublishingAsync_Should_SendBadConnection_If_SubscribeStreamExists()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var streamId = _fixture.Create<uint>();
            var commandChunkStreamId = Helpers.CreateRandomChunkStreamId();

            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();

            streamContext.StreamId.Returns(streamId);
            streamContext.ClientContext.Returns(clientContext);
            streamContext.PublishContext.Returns((IRtmpPublishStreamContext?)null);
            streamContext.SubscribeContext.Returns(subscribeStreamContext);
            streamContext.CommandChunkStreamId.Returns(commandChunkStreamId);

            // Act
            var result = await _sut.StartPublishingAsync(streamContext, streamPath, streamArguments);

            // Assert
            result.Result.Should().Be(PublishingStreamResult.AlreadySubscribing);

            _commandMessageSender.Received(1).SendCommandMessage(
                clientContext, streamId, commandChunkStreamId, "onStatus", 0, null,
                Helpers.CreateExpectedCommandProperties(RtmpStatusLevels.Error, RtmpStreamStatusCodes.PublishBadConnection),
                Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>());
        }

        [Fact]
        public async Task StartPublishingAsync_Should_SendBadConnection_If_PublishStreamExists()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var streamId = _fixture.Create<uint>();
            var commandChunkStreamId = Helpers.CreateRandomChunkStreamId();

            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();

            streamContext.StreamId.Returns(streamId);
            streamContext.ClientContext.Returns(clientContext);
            streamContext.PublishContext.Returns(publishStreamContext);
            streamContext.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);
            streamContext.CommandChunkStreamId.Returns(commandChunkStreamId);

            // Act
            var result = await _sut.StartPublishingAsync(streamContext, streamPath, streamArguments);

            // Assert
            result.Result.Should().Be(PublishingStreamResult.AlreadyPublishing);

            _commandMessageSender.Received(1).SendCommandMessage(
                clientContext, streamId, commandChunkStreamId, "onStatus", 0, null,
                Helpers.CreateExpectedCommandProperties(RtmpStatusLevels.Error, RtmpStreamStatusCodes.PublishBadConnection),
                Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>());
        }

        [Fact]
        public async Task StartPublishingAsync_Should_SendBadName_If_PublishStreamHasBeenRegistered()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var streamId = _fixture.Create<uint>();
            var commandChunkStreamId = Helpers.CreateRandomChunkStreamId();

            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();

            streamContext.StreamId.Returns(streamId);
            streamContext.ClientContext.Returns(clientContext);
            streamContext.PublishContext.Returns((IRtmpPublishStreamContext?)null);
            streamContext.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);
            streamContext.CommandChunkStreamId.Returns(commandChunkStreamId);

            var anotherStreamContext = Substitute.For<IRtmpStreamContext>();
            anotherStreamContext.PublishContext.Returns((IRtmpPublishStreamContext?)null);
            anotherStreamContext.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);
            await _sut.StartPublishingAsync(anotherStreamContext, streamPath, _fixture.Create<Dictionary<string, string>>());

            // Act
            var result = await _sut.StartPublishingAsync(streamContext, streamPath, streamArguments);

            // Assert
            result.Result.Should().Be(PublishingStreamResult.AlreadyExists);

            _commandMessageSender.Received(1).SendCommandMessage(
                clientContext, streamId, commandChunkStreamId, "onStatus", 0, null,
                Helpers.CreateExpectedCommandProperties(RtmpStatusLevels.Error, RtmpStreamStatusCodes.PublishBadName),
                Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>());
        }

        [Fact]
        public async Task StartDirectPublishingAsync_Should_ReturnAlreadyExists_If_PublishStreamHasBeenRegistered()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            publishStreamContext.StreamPath.Returns(streamPath);

            var anotherPublishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            anotherPublishStreamContext.StreamPath.Returns(streamPath);
            await _sut.StartDirectPublishingAsync(anotherPublishStreamContext);

            // Act
            var result = await _sut.StartDirectPublishingAsync(publishStreamContext);

            // Assert
            result.Result.Should().Be(PublishingStreamResult.AlreadyExists);
        }
    }
}
