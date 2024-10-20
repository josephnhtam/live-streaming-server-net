using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Auth;
using LiveStreamingServerNet.Rtmp.Server.Internal.Authorization.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Test.Utilities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.Commands
{
    public class RtmpPlayCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IRtmpCacherService _cacher;
        private readonly IStreamAuthorization _streamAuthorization;
        private readonly IRtmpStreamContext _streamContext;
        private readonly IRtmpSubscribeStreamContext _subscribeStreamContext;
        private readonly IRtmpPublishStreamContext _publishStreamContext;
        private readonly ILogger<RtmpPlayCommandHandler> _logger;
        private readonly RtmpPlayCommandHandler _sut;

        public RtmpPlayCommandHandlerTest()
        {
            _fixture = new Fixture();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientSessionContext>();
            _streamManager = Substitute.For<IRtmpStreamManagerService>();
            _commandMessageSender = Substitute.For<IRtmpCommandMessageSenderService>();
            _userControlMessageSender = Substitute.For<IRtmpUserControlMessageSenderService>();
            _cacher = Substitute.For<IRtmpCacherService>();
            _streamAuthorization = Substitute.For<IStreamAuthorization>();
            _streamContext = Substitute.For<IRtmpStreamContext>();
            _subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            _publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            _logger = Substitute.For<ILogger<RtmpPlayCommandHandler>>();

            _streamContext.ClientContext.Returns(_clientContext);
            _subscribeStreamContext.StreamContext.Returns(_streamContext);

            _commandMessageSender.When(x =>
                x.SendCommandMessage(Arg.Any<IRtmpClientSessionContext>(), Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<string>(), Arg.Any<double>(),
                    Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IReadOnlyList<object?>>(),
                    Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>()))
                .Do(x => x.Arg<Action<bool>>()?.Invoke(true));

            _sut = new RtmpPlayCommandHandler(
                _streamManager,
                _commandMessageSender,
                _userControlMessageSender,
                _cacher,
                _streamAuthorization,
                _logger
            );
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnFalse_When_StreamIsNotYetCreated()
        {
            // Arrange
            var transactionId = 0.0;
            var commandObject = new Dictionary<string, object>();
            var streamName = "streamName?password=123456";
            var command = new RtmpPlayCommand(transactionId, commandObject, streamName, 0, 0, false);

            _clientContext.GetStreamContext(Arg.Any<uint>()).Returns((IRtmpStreamContext?)null);

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

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStreamContext(streamId).Returns(_streamContext);
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
        public async Task HandleAsync_Should_SendCaches_If_StreamIsSubscribedSuccessfully(bool publishStreamExists, bool gopCacheActivated)
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
            var command = new RtmpPlayCommand(transactionId, commandObject, streamName, 0, 0, false);

            _clientContext.GetStreamContext(streamId).Returns(_streamContext);
            _clientContext.AppName.Returns(appName);

            _chunkStreamContext.ChunkStreamId.Returns(chunkStreamId);
            _chunkStreamContext.Timestamp.Returns(timestamp);
            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);

            _streamContext.StreamId.Returns(streamId);
            _streamContext.SubscribeContext.Returns(_subscribeStreamContext);

            if (publishStreamExists)
            {
                _publishStreamContext.GroupOfPicturesCacheActivated.Returns(gopCacheActivated);
                _streamManager.GetPublishStreamContext(streamPath).Returns(_publishStreamContext);
            }

            _streamAuthorization.AuthorizeSubscribingAsync(_clientContext, streamPath, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(AuthorizationResult.Authorized());

            _streamManager.StartSubscribingAsync(
                _streamContext, streamPath, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(x =>
                {
                    return (SubscribingStreamResult.Succeeded, publishStreamExists ? _publishStreamContext : null);
                })
                .AndDoes(x =>
                {
                    _subscribeStreamContext.StreamPath.Returns(x.Arg<string>());
                    _subscribeStreamContext.StreamArguments.Returns(x.Arg<IReadOnlyDictionary<string, string>>());
                });

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            Received.InOrder(() =>
            {
                if (publishStreamExists)
                {
                    _cacher.Received(1).SendCachedStreamMetaDataMessage(
                        _subscribeStreamContext, _publishStreamContext);

                    _cacher.Received(1).SendCachedHeaderMessages(
                        _subscribeStreamContext, _publishStreamContext);

                    if (gopCacheActivated)
                        _cacher.Received(1).SendCachedGroupOfPictures(
                            _subscribeStreamContext, _publishStreamContext);
                }

                _subscribeStreamContext.Received(1).CompleteInitialization();
            });

            result.Should().BeTrue();
        }
    }
}
