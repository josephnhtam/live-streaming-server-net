using AutoFixture;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.Services
{
    public class RtmpMediaMessageBroadcasterServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpChunkMessageWriterService _chunkMessageWriter;
        private readonly IRtmpMediaMessageInterceptionService _interception;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IPacketDiscarder _mediaPacketDiscarder;
        private readonly IMediaPacketDiscarderFactory _mediaPacketDiscarderFactory;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger<RtmpMediaMessageBroadcasterService> _logger;
        private readonly IRtmpMediaMessageBroadcasterService _sut;

        public RtmpMediaMessageBroadcasterServiceTest()
        {
            _fixture = new Fixture();
            _chunkMessageWriter = Substitute.For<IRtmpChunkMessageWriterService>();
            _interception = Substitute.For<IRtmpMediaMessageInterceptionService>();
            _dataBufferPool = new DataBufferPool(Options.Create(new DataBufferPoolConfiguration()));
            _mediaPacketDiscarder = Substitute.For<IPacketDiscarder>();
            _mediaPacketDiscarderFactory = Substitute.For<IMediaPacketDiscarderFactory>();
            _config = new RtmpServerConfiguration();
            _logger = Substitute.For<ILogger<RtmpMediaMessageBroadcasterService>>();

            _mediaPacketDiscarderFactory.Create(Arg.Any<uint>()).Returns(_mediaPacketDiscarder);
            _mediaPacketDiscarder.ShouldDiscardPacket(Arg.Any<bool>(), Arg.Any<long>(), Arg.Any<long>())
                .Returns(false);

            _sut = new RtmpMediaMessageBroadcasterService(
                _chunkMessageWriter,
                _interception,
                _dataBufferPool,
                _mediaPacketDiscarderFactory,
                Options.Create(_config),
                _logger);
        }

        [Fact]
        public async Task BroadcastMediaMessageAsync_Should_SendMediaMessageToClient_After_ClientSubscriptionIsInitialized()
        {
            // Arrange
            var subscriber = Substitute.For<ISessionHandle>();

            var subscriber_clientContext = Substitute.For<IRtmpClientSessionContext>();
            subscriber_clientContext.Client.Returns(subscriber);

            var subscriber_streamContext = Substitute.For<IRtmpStreamContext>();
            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            subscriber_streamContext.StreamId.Returns(_fixture.Create<uint>());
            subscriber_streamContext.ClientContext.Returns(subscriber_clientContext);
            subscriber_subscribeStreamContext.IsReceivingVideo.Returns(true);
            subscriber_subscribeStreamContext.IsReceivingAudio.Returns(true);
            subscriber_subscribeStreamContext.StreamContext.Returns(subscriber_streamContext);

            var publisher_streamContext = Substitute.For<IRtmpStreamContext>();
            var publisher_publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            publisher_streamContext.StreamId.Returns(_fixture.Create<uint>());
            publisher_streamContext.PublishContext.Returns(publisher_publishStreamContext);

            var subscriber_subscribeStreamContexts = new List<IRtmpSubscribeStreamContext> { subscriber_subscribeStreamContext };
            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();
            var isSkippable = false;
            var payloadBuffer = _fixture.Create<DataBuffer>();

            var tcs = new TaskCompletionSource();

            subscriber.When(x => x.SendAsync(Arg.Any<IRentedBuffer>())).Do(_ => tcs.TrySetResult());

            // Act
            _sut.RegisterClient(subscriber_clientContext);
            await _sut.BroadcastMediaMessageAsync(publisher_publishStreamContext, subscriber_subscribeStreamContexts, mediaType, timestamp, isSkippable, payloadBuffer);

            // Assert
            await tcs.Task;

            Received.InOrder(() =>
            {
                subscriber_subscribeStreamContext.Received(1).UntilInitializationCompleteAsync();
                subscriber.Received(1).SendAsync(Arg.Any<IRentedBuffer>());
            });
        }
    }
}
