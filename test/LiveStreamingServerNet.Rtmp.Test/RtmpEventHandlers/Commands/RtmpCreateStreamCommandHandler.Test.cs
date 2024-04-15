using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Commands
{
    public class RtmpCreateStreamCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientContext _clientContext;
        private readonly RtmpCreateStreamCommandHandler _sut;

        public RtmpCreateStreamCommandHandlerTest()
        {
            _fixture = new Fixture();
            _commandMessageSender = Substitute.For<IRtmpCommandMessageSenderService>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientContext>();

            _sut = new RtmpCreateStreamCommandHandler(_commandMessageSender);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTrueAndRespond_When_NoStreamCreated()
        {
            // Arrange
            var transactionId = _fixture.Create<double>();
            var streamId = _fixture.Create<uint>();
            var commandObject = new Dictionary<string, object>();
            var command = new RtmpCreateStreamCommand(transactionId, commandObject);

            _clientContext.CreateNewStream().Returns(streamId);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();

            _clientContext.Received(1).CreateNewStream();

            _commandMessageSender.Received(1).SendCommandMessage(
                _clientContext, 0, "_result", transactionId, Arg.Any<IReadOnlyDictionary<string, object>>(),
                Arg.Is<IReadOnlyList<object?>>(x => (uint)x.First()! == streamId));
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTrueWithoutResponse_When_StreamIsCreated()
        {
            // Arrange
            var transactionId = _fixture.Create<double>();
            var streamId = _fixture.Create<uint>();
            var commandObject = new Dictionary<string, object>();
            var command = new RtmpCreateStreamCommand(transactionId, commandObject);

            _clientContext.StreamId.Returns(streamId);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();

            _clientContext.DidNotReceive().CreateNewStream();

            _commandMessageSender.DidNotReceive().SendCommandMessage(
                _clientContext, 0, "_result", transactionId, Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IReadOnlyList<object?>>());
        }
    }
}
