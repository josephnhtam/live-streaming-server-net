﻿using AutoFixture;
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
    public class RtmpPublishCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IStreamAuthorization _streamAuthorization;
        private readonly IRtmpStreamContext _streamContext;
        private readonly IRtmpPublishStreamContext _publishStreamContext;
        private readonly ILogger<RtmpPublishCommandHandler> _logger;
        private readonly RtmpPublishCommandHandler _sut;

        public RtmpPublishCommandHandlerTest()
        {
            _fixture = new Fixture();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientSessionContext>();
            _streamManager = Substitute.For<IRtmpStreamManagerService>();
            _commandMessageSender = Substitute.For<IRtmpCommandMessageSenderService>();
            _userControlMessageSender = Substitute.For<IRtmpUserControlMessageSenderService>();
            _streamAuthorization = Substitute.For<IStreamAuthorization>();
            _streamContext = Substitute.For<IRtmpStreamContext>();
            _publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            _logger = Substitute.For<ILogger<RtmpPublishCommandHandler>>();

            _streamContext.ClientContext.Returns(_clientContext);
            _publishStreamContext.StreamContext.Returns(_streamContext);

            _commandMessageSender.When(x =>
                x.SendCommandMessage(Arg.Any<IRtmpClientSessionContext>(), Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<string>(), Arg.Any<double>(),
                    Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IReadOnlyList<object?>>(),
                    Arg.Any<AmfEncodingType>(), Arg.Any<Action<bool>>()))
                .Do(x => x.Arg<Action<bool>>()?.Invoke(true));

            _sut = new RtmpPublishCommandHandler(
                _streamManager,
                _commandMessageSender,
                _userControlMessageSender,
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
            var publishingType = "live";
            var command = new RtmpPublishCommand(transactionId, commandObject, streamName, publishingType);

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
            var streamName = "streamName?password=123456";
            var streamId = _fixture.Create<uint>();
            var publishingType = "live";
            var command = new RtmpPublishCommand(transactionId, commandObject, streamName, publishingType);

            var streamPath = "/appName/streamName";

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStreamContext(streamId).Returns(_streamContext);
            _clientContext.AppName.Returns("appName");
            _streamContext.StreamId.Returns(streamId);

            _streamAuthorization.AuthorizePublishingAsync(_clientContext, streamPath, publishingType, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(AuthorizationResult.Unauthorized("testing"));

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_Should_DispatchEvent_If_StreamIsPublishedSuccessfully()
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
            var command = new RtmpPublishCommand(transactionId, commandObject, streamName, publishingType);

            var subsciber_streamId = _fixture.Create<uint>();
            var subscriber_clientContext = Substitute.For<IRtmpClientSessionContext>();
            var subscriber_streamContext = Substitute.For<IRtmpStreamContext>();
            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();

            subscriber_subscribeStreamContext.StreamContext.Returns(subscriber_streamContext);
            subscriber_streamContext.ClientContext.Returns(subscriber_clientContext);
            subscriber_streamContext.StreamId.Returns(subsciber_streamId);

            _clientContext.GetStreamContext(streamId).Returns(_streamContext);
            _clientContext.AppName.Returns(appName);
            _chunkStreamContext.ChunkStreamId.Returns(chunkStreamId);
            _chunkStreamContext.Timestamp.Returns(timestamp);
            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _streamContext.StreamId.Returns(streamId);

            _streamAuthorization.AuthorizePublishingAsync(
                _clientContext, streamPath, publishingType, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(AuthorizationResult.Authorized())
                .AndDoes(x =>
                {
                    _streamContext.PublishContext.Returns(_publishStreamContext);
                    _publishStreamContext.StreamPath.Returns(x.ArgAt<string>(1));
                    _publishStreamContext.StreamArguments.Returns(x.Arg<IReadOnlyDictionary<string, string>>());
                });

            _streamManager.StartPublishingAsync(_streamContext, streamPath, Helpers.CreateExpectedStreamArguments("password", "123456"))
                .Returns(x => (PublishingStreamResult.Succeeded, new List<IRtmpSubscribeStreamContext> { subscriber_subscribeStreamContext }));

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();
        }
    }
}
