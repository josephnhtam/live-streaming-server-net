using AutoFixture;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test.Services
{
    public class FlvMediaTagBroadcasterServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IFlvStreamContext _streamContext;
        private readonly IMediaPacketDiscarderFactory _mediaPacketDiscarderFactory;
        private readonly IPacketDiscarder _mediaPacketDiscarder;
        private readonly IFlvMediaTagSenderService _mediaTagSender;
        private readonly ILogger<FlvMediaTagBroadcasterService> _logger;
        private readonly FlvMediaTagBroadcasterService _sut;

        public FlvMediaTagBroadcasterServiceTest()
        {
            _fixture = new Fixture();
            _streamContext = Substitute.For<IFlvStreamContext>();
            _mediaPacketDiscarderFactory = Substitute.For<IMediaPacketDiscarderFactory>();
            _mediaPacketDiscarder = Substitute.For<IPacketDiscarder>();
            _mediaTagSender = Substitute.For<IFlvMediaTagSenderService>();
            _logger = Substitute.For<ILogger<FlvMediaTagBroadcasterService>>();

            _mediaPacketDiscarderFactory.Create(Arg.Any<string>()).Returns(_mediaPacketDiscarder);
            _mediaPacketDiscarder.ShouldDiscardPacket(Arg.Any<bool>(), Arg.Any<long>(), Arg.Any<long>()).Returns(false);

            _sut = new FlvMediaTagBroadcasterService(
                _mediaPacketDiscarderFactory,
                _mediaTagSender,
                _logger);
        }

        [Fact]
        public void BroadcastMediaTagAsync_Should_ClaimRentedBuffer()
        {
            // Arrange
            var subscribers = new List<IFlvClient> { Substitute.For<IFlvClient>() };
            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();
            var isSkippable = _fixture.Create<bool>();
            var rentedBuffer = Substitute.For<IRentedBuffer>();

            // Act
            _sut.BroadcastMediaTagAsync(_streamContext, subscribers, mediaType, timestamp, isSkippable, rentedBuffer);

            // Assert
            rentedBuffer.Received(1).Claim(subscribers.Count);
        }

        [Fact]
        public async Task BroadcastMediaTagAsync_Should_SendMediaTagToClient_After_ClientIsInitialized()
        {
            // Arrange
            var client = Substitute.For<IFlvClient>();

            var subscribers = new List<IFlvClient> { client };
            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();
            var isSkippable = false;
            var rentedBuffer = Substitute.For<IRentedBuffer>();

            var tcs = new TaskCompletionSource();

            _mediaTagSender.When(x => x.SendMediaTagAsync(
                client, mediaType, rentedBuffer.Buffer, rentedBuffer.Size, timestamp, Arg.Any<CancellationToken>()))
                .Do(_ => tcs.TrySetResult());

            // Act
            _sut.RegisterClient(client);
            await _sut.BroadcastMediaTagAsync(_streamContext, subscribers, mediaType, timestamp, isSkippable, rentedBuffer);

            // Assert
            await tcs.Task;

            Received.InOrder(() =>
            {
                client.Received(1).UntilInitializationCompleteAsync();
                _mediaTagSender.When(x => x.SendMediaTagAsync(
                    client, mediaType, rentedBuffer.Buffer, rentedBuffer.Size, timestamp, Arg.Any<CancellationToken>()));
            });
        }
    }
}
