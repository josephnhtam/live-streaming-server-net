using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Commands
{
    public class RtmpConnectCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpProtocolControlMessageSenderService _protocolControlMessageSender;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger<RtmpConnectCommandHandler> _logger;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientContext _clientContext;
        private readonly RtmpConnectCommandHandler _sut;

        public RtmpConnectCommandHandlerTest()
        {
            _fixture = new Fixture();
            _protocolControlMessageSender = Substitute.For<IRtmpProtocolControlMessageSenderService>();
            _commandMessageSender = Substitute.For<IRtmpCommandMessageSenderService>();
            _eventDispatcher = Substitute.For<IRtmpServerConnectionEventDispatcher>();
            _config = _fixture.Create<RtmpServerConfiguration>();
            _logger = Substitute.For<ILogger<RtmpConnectCommandHandler>>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientContext>();

            _sut = new RtmpConnectCommandHandler(
                _protocolControlMessageSender, _commandMessageSender, _eventDispatcher,
                Options.Create(_config), _logger);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTrue_When_ParametersAreCorrect()
        {
            // Arrange
            var transactionId = _fixture.Create<double>();
            var appName = _fixture.Create<string>();
            var commandObject = new Dictionary<string, object>() { ["app"] = appName };
            var argument = new Dictionary<string, object>();
            var command = new RtmpConnectCommand(transactionId, commandObject, argument);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();

            _clientContext.Received(1).AppName = appName;
            _protocolControlMessageSender.Received(1).SetChunkSize(_clientContext, _config.OutChunkSize);
            _protocolControlMessageSender.Received(1).WindowAcknowledgementSize(_clientContext, _config.OutAcknowledgementWindowSize);
            _protocolControlMessageSender.Received(1).SetClientBandwidth(_clientContext, _config.ClientBandwidth, Arg.Any<RtmpClientBandwidthLimitType>());

            _commandMessageSender.Received(1).SendCommandMessage(
                _clientContext, 3, "_result", transactionId, Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IReadOnlyList<object?>>());

            _ = _eventDispatcher.Received(1).RtmpClientConnectedAsync(
                _clientContext,
                Arg.Is<IReadOnlyDictionary<string, object>>(x => x.SequenceEqual(commandObject)),
                Arg.Is<IReadOnlyDictionary<string, object>>(x => x.SequenceEqual(argument))
            );
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnFalse_When_AlreadyCorrected()
        {
            // Arrange
            var transactionId = _fixture.Create<double>();
            var appName = _fixture.Create<string>();
            var commandObject = new Dictionary<string, object>() { ["app"] = appName };
            var argument = new Dictionary<string, object>();
            var command = new RtmpConnectCommand(transactionId, commandObject, argument);

            _clientContext.AppName.Returns(_fixture.Create<string>());

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeFalse();

            _clientContext.DidNotReceive().AppName = Arg.Any<string>();
            _protocolControlMessageSender.DidNotReceive().SetChunkSize(Arg.Any<IRtmpClientContext>(), Arg.Any<uint>());
            _protocolControlMessageSender.DidNotReceive().WindowAcknowledgementSize(Arg.Any<IRtmpClientContext>(), Arg.Any<uint>());
            _protocolControlMessageSender.DidNotReceive().SetClientBandwidth(
                Arg.Any<IRtmpClientContext>(), Arg.Any<uint>(), Arg.Any<RtmpClientBandwidthLimitType>());

            _commandMessageSender.DidNotReceive().SendCommandMessage(
                Arg.Any<IRtmpClientContext>(), Arg.Any<uint>(), Arg.Any<string>(), Arg.Any<double>(),
                Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IReadOnlyList<object?>>());

            _ = _eventDispatcher.DidNotReceive().RtmpClientConnectedAsync(
                _clientContext,
                Arg.Any<IReadOnlyDictionary<string, object>>(),
                Arg.Any<IReadOnlyDictionary<string, object>>()
            );
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnFalse_When_AppNameIsInvalid()
        {
            // Arrange
            var transactionId = _fixture.Create<double>();
            var appName = string.Empty;
            var commandObject = new Dictionary<string, object>() { ["app"] = appName };
            var argument = new Dictionary<string, object>();
            var command = new RtmpConnectCommand(transactionId, commandObject, argument);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeFalse();

            _clientContext.DidNotReceive().AppName = Arg.Any<string>();
            _protocolControlMessageSender.DidNotReceive().SetChunkSize(Arg.Any<IRtmpClientContext>(), Arg.Any<uint>());
            _protocolControlMessageSender.DidNotReceive().WindowAcknowledgementSize(Arg.Any<IRtmpClientContext>(), Arg.Any<uint>());
            _protocolControlMessageSender.DidNotReceive().SetClientBandwidth(
                Arg.Any<IRtmpClientContext>(), Arg.Any<uint>(), Arg.Any<RtmpClientBandwidthLimitType>());

            _commandMessageSender.DidNotReceive().SendCommandMessage(
                Arg.Any<IRtmpClientContext>(), Arg.Any<uint>(), Arg.Any<string>(), Arg.Any<double>(),
                Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IReadOnlyList<object?>>());

            _ = _eventDispatcher.DidNotReceive().RtmpClientConnectedAsync(
                _clientContext,
                Arg.Any<IReadOnlyDictionary<string, object>>(),
                Arg.Any<IReadOnlyDictionary<string, object>>()
            );
        }
    }
}
