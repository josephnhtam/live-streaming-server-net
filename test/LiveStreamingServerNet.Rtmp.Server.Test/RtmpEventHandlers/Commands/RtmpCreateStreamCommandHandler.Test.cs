﻿using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.Commands
{
    public class RtmpCreateStreamCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly RtmpCreateStreamCommandHandler _sut;

        public RtmpCreateStreamCommandHandlerTest()
        {
            _fixture = new Fixture();
            _commandMessageSender = Substitute.For<IRtmpCommandMessageSenderService>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientSessionContext>();

            _sut = new RtmpCreateStreamCommandHandler(_commandMessageSender);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTrueAndRespond()
        {
            // Arrange
            var transactionId = _fixture.Create<double>();
            var streamId = _fixture.Create<uint>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var commandObject = new Dictionary<string, object>();
            var command = new RtmpCreateStreamCommand(transactionId, commandObject);

            streamContext.StreamId.Returns(streamId);
            _clientContext.CreateStreamContext().Returns(streamContext);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();

            _clientContext.Received(1).CreateStreamContext();

            _commandMessageSender.Received(1).SendCommandMessage(
                _clientContext, 0, 0, "_result", transactionId, Arg.Any<IReadOnlyDictionary<string, object>>(),
                Arg.Is<IReadOnlyList<object?>>(x => (uint)x.First()! == streamId));
        }
    }
}
