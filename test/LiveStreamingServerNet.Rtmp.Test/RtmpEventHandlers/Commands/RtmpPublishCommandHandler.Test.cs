using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Authorization.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Test.Utilities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Commands
{
    public class RtmpPublishCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientContext _clientContext;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly IStreamAuthorization _streamAuthorization;
        private readonly IRtmpPublishStreamContext _publishStreamContext;
        private readonly ILogger<RtmpPublishCommandHandler> _logger;
        private readonly RtmpPublishCommandHandler _sut;

        public RtmpPublishCommandHandlerTest()
        {
            _fixture = new Fixture();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientContext>();
            _streamManager = Substitute.For<IRtmpStreamManagerService>();
            _commandMessageSender = Substitute.For<IRtmpCommandMessageSenderService>();
            _eventDispatcher = Substitute.For<IRtmpServerStreamEventDispatcher>();
            _streamAuthorization = Substitute.For<IStreamAuthorization>();
            _publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            _logger = Substitute.For<ILogger<RtmpPublishCommandHandler>>();

            _commandMessageSender.When(x =>
                x.SendCommandMessage(Arg.Any<IRtmpClientContext>(), Arg.Any<uint>(), Arg.Any<string>(), Arg.Any<double>(),
                    Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IReadOnlyList<object?>>(),
                    Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>()))
                .Do(x => x.Arg<Action<bool>>()?.Invoke(true));

            _sut = new RtmpPublishCommandHandler(
                _streamManager,
                _commandMessageSender,
                _eventDispatcher,
                _streamAuthorization,
                _logger
            );
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnFalse_When_StreamIdIsNotYetCreated()
        {
            // Arrange
            var transactionId = 0.0;
            var commandObject = new Dictionary<string, object>();
            var streamName = "streamName?password=123456";
            var publishingType = "live";
            var command = new RtmpPublishCommand(transactionId, commandObject, streamName, publishingType);

            _clientContext.StreamId.Returns((uint?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnFalse_If_NotAuthorized()
        {
            // Arrange
            var transactionId = 0.0;
            var commandObject = new Dictionary<string, object>();
            var streamName = "streamName?password=123456";
            var streamId = _fixture.Create<uint>();
            var publishingType = "live";
            var command = new RtmpPublishCommand(transactionId, commandObject, streamName, publishingType);

            var streamPath = "/appName/streamName";

            _clientContext.StreamId.Returns(streamId);
            _clientContext.AppName.Returns("appName");

            _streamAuthorization.AuthorizePublishingAsync(_clientContext, streamPath, publishingType, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(AuthorizationResult.Unauthorized("testing"));

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task HandleAsync_Should_SendPlayStartAndCaches_If_AuthorizedAndStreamIsSubscribedSuccesfully(bool publishStreamExists, bool gopCacheActivated)
        {
            // Arrange
            var transactionId = 0.0;
            var commandObject = new Dictionary<string, object>();
            var appName = "appName";
            var streamName = "streamName?password=123456";
            var streamPath = "/appName/streamName";
            var streamId = _fixture.Create<uint>();
            var publishingType = "live";
            var chunkStreamId = Helpers.CreateRandomChunkStreamId();
            var timestamp = _fixture.Create<uint>();
            var messageStreamId = _fixture.Create<uint>();
            var command = new RtmpPublishCommand(transactionId, commandObject, streamName, publishingType);

            _clientContext.StreamId.Returns(streamId);
            _clientContext.AppName.Returns(appName);
            _chunkStreamContext.ChunkStreamId.Returns(chunkStreamId);
            _chunkStreamContext.MessageHeader.Timestamp.Returns(timestamp);
            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(messageStreamId);

            if (publishStreamExists)
            {
                _publishStreamContext.GroupOfPicturesCacheActivated.Returns(gopCacheActivated);
                _streamManager.GetPublishStreamContext(streamPath).Returns(_publishStreamContext);
            }

            _streamAuthorization.AuthorizePublishingAsync(
                _clientContext, streamPath, publishingType, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(AuthorizationResult.Authorized())
                .AndDoes(x =>
                {
                    _clientContext.PublishStreamContext!.StreamPath.Returns(x.ArgAt<string>(1));
                    _clientContext.PublishStreamContext!.StreamArguments.Returns(x.Arg<IReadOnlyDictionary<string, string>>());
                });

            _streamManager.StartPublishingStream(_clientContext, streamPath,
                Helpers.CreateExpectedStreamArguments("password", "123456"), out Arg.Any<IList<IRtmpClientContext>>())
                .Returns(PublishingStreamResult.Succeeded);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            Received.InOrder(() =>
            {
                _commandMessageSender.Received(1).SendCommandMessage(
                    _clientContext, chunkStreamId, "onStatus", 0, null,
                    Helpers.CreateExpectedCommandProperties(RtmpArgumentValues.Status, RtmpStatusCodes.PublishStart),
                    Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>());

                _ = _eventDispatcher.Received(1).RtmpStreamPublishedAsync(
                    _clientContext, streamPath, Helpers.CreateExpectedStreamArguments("password", "123456"));
            });

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(PublishingStreamResult.AlreadyPublishing)]
        [InlineData(PublishingStreamResult.AlreadySubscribing)]
        internal async Task HandleAsync_Should_SendError_If_AuthorizedButStreamSubscriptionNotSuccesful(PublishingStreamResult publishingResult)
        {
            // Arrange
            var transactionId = 0.0;
            var commandObject = new Dictionary<string, object>();
            var appName = "appName";
            var streamName = "streamName?password=123456";
            var streamPath = "/appName/streamName";
            var streamId = _fixture.Create<uint>();
            var publishingType = "live";
            var chunkStreamId = Helpers.CreateRandomChunkStreamId();
            var timestamp = _fixture.Create<uint>();
            var messageStreamId = _fixture.Create<uint>();
            var command = new RtmpPublishCommand(transactionId, commandObject, streamName, publishingType);

            _clientContext.StreamId.Returns(streamId);
            _clientContext.AppName.Returns(appName);
            _chunkStreamContext.ChunkStreamId.Returns(chunkStreamId);

            _streamAuthorization.AuthorizePublishingAsync(
                _clientContext, streamPath, publishingType, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(AuthorizationResult.Authorized())
                .AndDoes(x =>
                {
                    _clientContext.PublishStreamContext!.StreamPath.Returns(x.ArgAt<string>(1));
                    _clientContext.PublishStreamContext!.StreamArguments.Returns(x.Arg<IReadOnlyDictionary<string, string>>());
                });

            _streamManager.StartPublishingStream(_clientContext, streamPath,
                Helpers.CreateExpectedStreamArguments("password", "123456"), out Arg.Any<IList<IRtmpClientContext>>())
                .Returns(publishingResult);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            _commandMessageSender.Received(1).SendCommandMessage(
                _clientContext, chunkStreamId, "onStatus", 0, null,
                Helpers.CreateExpectedCommandProperties(RtmpArgumentValues.Error, RtmpStatusCodes.PublishBadConnection),
                Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>());

            result.Should().BeTrue();
        }
    }
}
