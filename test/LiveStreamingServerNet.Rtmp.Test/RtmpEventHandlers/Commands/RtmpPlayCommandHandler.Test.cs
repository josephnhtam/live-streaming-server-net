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
    public class RtmpPlayCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientContext _clientContext;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly IStreamAuthorization _streamAuthorization;
        private readonly IRtmpPublishStreamContext _publishStreamContext;
        private readonly ILogger<RtmpPlayCommandHandler> _logger;
        private readonly RtmpPlayCommandHandler _sut;

        public RtmpPlayCommandHandlerTest()
        {
            _fixture = new Fixture();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientContext>();
            _streamManager = Substitute.For<IRtmpStreamManagerService>();
            _commandMessageSender = Substitute.For<IRtmpCommandMessageSenderService>();
            _mediaMessageCacher = Substitute.For<IRtmpMediaMessageCacherService>();
            _eventDispatcher = Substitute.For<IRtmpServerStreamEventDispatcher>();
            _streamAuthorization = Substitute.For<IStreamAuthorization>();
            _publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            _logger = Substitute.For<ILogger<RtmpPlayCommandHandler>>();

            _commandMessageSender.When(x =>
                x.SendCommandMessage(Arg.Any<IRtmpClientContext>(), Arg.Any<uint>(), Arg.Any<string>(), Arg.Any<double>(),
                    Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IReadOnlyList<object?>>(),
                    Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>()))
                .Do(x => x.Arg<Action<bool>>()?.Invoke(true));

            _sut = new RtmpPlayCommandHandler(
                _streamManager,
                _commandMessageSender,
                _mediaMessageCacher,
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
            var command = new RtmpPlayCommand(transactionId, commandObject, streamName, 0, 0, false);

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
            var appName = "appName";
            var streamName = "streamName?password=123456";
            var streamPath = "/appName/streamName";
            var streamId = _fixture.Create<uint>();
            var command = new RtmpPlayCommand(transactionId, commandObject, streamName, 0, 0, false);

            _clientContext.StreamId.Returns(streamId);
            _clientContext.AppName.Returns(appName);

            _streamAuthorization.AuthorizeSubscribingAsync(_clientContext, streamPath, Helpers.CreateExpectedStreamArguments("password", "123456"))
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
            var chunkStreamId = Helpers.CreateRandomChunkStreamId();
            var timestamp = _fixture.Create<uint>();
            var messageStreamId = _fixture.Create<uint>();
            var command = new RtmpPlayCommand(transactionId, commandObject, streamName, 0, 0, false);

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

            _streamAuthorization.AuthorizeSubscribingAsync(_clientContext, streamPath, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(AuthorizationResult.Authorized())
                .AndDoes(x =>
                {
                    _clientContext.StreamSubscriptionContext!.StreamPath.Returns(x.Arg<string>());
                    _clientContext.StreamSubscriptionContext!.StreamArguments.Returns(x.Arg<IReadOnlyDictionary<string, string>>());
                });

            _streamManager.StartSubscribingStream(
                _clientContext, chunkStreamId, streamPath, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(SubscribingStreamResult.Succeeded);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            Received.InOrder(() =>
            {
                _commandMessageSender.Received(1).SendCommandMessage(
                    _clientContext, chunkStreamId, "onStatus", 0, null,
                    Helpers.CreateExpectedCommandProperties(RtmpArgumentValues.Status, RtmpStatusCodes.PlayStart),
                    Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>());

                if (publishStreamExists)
                {
                    _mediaMessageCacher.Received(1).SendCachedStreamMetaDataMessage(
                        _clientContext, _publishStreamContext, timestamp, messageStreamId);

                    _mediaMessageCacher.Received(1).SendCachedHeaderMessages(
                        _clientContext, _publishStreamContext, messageStreamId);

                    if (gopCacheActivated)
                        _mediaMessageCacher.Received(1).SendCachedGroupOfPictures(
                            _clientContext, _publishStreamContext, messageStreamId);
                }

                _clientContext.Received(1).StreamSubscriptionContext!.CompleteInitialization();

                _ = _eventDispatcher.Received(1).RtmpStreamSubscribedAsync(
                    _clientContext, streamPath, Helpers.CreateExpectedStreamArguments("password", "123456"));
            });

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(SubscribingStreamResult.AlreadyPublishing)]
        [InlineData(SubscribingStreamResult.AlreadySubscribing)]
        internal async Task HandleAsync_Should_SendError_If_AuthorizedButStreamSubscriptionNotSuccesful(SubscribingStreamResult subscribingResult)
        {
            // Arrange
            var transactionId = 0.0;
            var commandObject = new Dictionary<string, object>();
            var appName = "appName";
            var streamName = "streamName?password=123456";
            var streamPath = "/appName/streamName";
            var streamId = _fixture.Create<uint>();
            var chunkStreamId = Helpers.CreateRandomChunkStreamId();
            var timestamp = _fixture.Create<uint>();
            var messageStreamId = _fixture.Create<uint>();
            var command = new RtmpPlayCommand(transactionId, commandObject, streamName, 0, 0, false);

            _clientContext.StreamId.Returns(streamId);
            _clientContext.AppName.Returns(appName);
            _chunkStreamContext.ChunkStreamId.Returns(chunkStreamId);

            _streamAuthorization.AuthorizeSubscribingAsync(
                _clientContext, streamPath, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(AuthorizationResult.Authorized())
                .AndDoes(x =>
                {
                    _clientContext.StreamSubscriptionContext!.StreamPath.Returns(x.Arg<string>());
                    _clientContext.StreamSubscriptionContext!.StreamArguments.Returns(x.Arg<IReadOnlyDictionary<string, string>>());
                });

            _streamManager.StartSubscribingStream(_clientContext, chunkStreamId, streamPath, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(subscribingResult);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            _commandMessageSender.Received(1).SendCommandMessage(
                _clientContext, chunkStreamId, "onStatus", 0, null,
                Helpers.CreateExpectedCommandProperties(RtmpArgumentValues.Error, RtmpStatusCodes.PlayBadConnection),
                Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>());

            result.Should().BeTrue();
        }
    }
}
