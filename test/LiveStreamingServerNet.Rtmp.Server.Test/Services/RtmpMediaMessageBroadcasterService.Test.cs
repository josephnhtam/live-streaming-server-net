using AutoFixture;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.MediaPackageDiscarding.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
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
        private readonly IMediaPackageDiscarder _mediaPackageDiscarder;
        private readonly IMediaPackageDiscarderFactory _mediaPackageDiscarderFactory;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger<RtmpMediaMessageBroadcasterService> _logger;
        private readonly IRtmpMediaMessageBroadcasterService _sut;

        public RtmpMediaMessageBroadcasterServiceTest()
        {
            _fixture = new Fixture();
            _chunkMessageWriter = Substitute.For<IRtmpChunkMessageWriterService>();
            _interception = Substitute.For<IRtmpMediaMessageInterceptionService>();
            _dataBufferPool = new DataBufferPool(Options.Create(new DataBufferPoolConfiguration()));
            _mediaPackageDiscarder = Substitute.For<IMediaPackageDiscarder>();
            _mediaPackageDiscarderFactory = Substitute.For<IMediaPackageDiscarderFactory>();
            _config = new RtmpServerConfiguration();
            _logger = Substitute.For<ILogger<RtmpMediaMessageBroadcasterService>>();

            _mediaPackageDiscarderFactory.Create(Arg.Any<uint>()).Returns(_mediaPackageDiscarder);
            _mediaPackageDiscarder.ShouldDiscardMediaPackage(Arg.Any<bool>(), Arg.Any<long>(), Arg.Any<long>())
                .Returns(false);

            _sut = new RtmpMediaMessageBroadcasterService(
                _chunkMessageWriter,
                _interception,
                _dataBufferPool,
                _mediaPackageDiscarderFactory,
                Options.Create(_config),
                _logger);
        }

        [Fact]
        public async Task BroadcastMediaMessageAsync_Should_SendMediaMessageToClient_After_ClientSubscriptionIsInitialized()
        {
            // Arrange
            var subscriber = Substitute.For<ISessionHandle>();

            var subscriberContext = Substitute.For<IRtmpClientSessionContext>();
            subscriberContext.Client.Returns(subscriber);

            var subscribeStream = Substitute.For<IRtmpStream>();
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            subscribeStream.Id.Returns(_fixture.Create<uint>());
            subscribeStream.ClientContext.Returns(subscriberContext);
            subscribeStreamContext.IsReceivingVideo.Returns(true);
            subscribeStreamContext.IsReceivingAudio.Returns(true);
            subscribeStreamContext.Stream.Returns(subscribeStream);

            var publishStream = Substitute.For<IRtmpStream>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            publishStream.Id.Returns(_fixture.Create<uint>());
            publishStream.PublishContext.Returns(publishStreamContext);

            var subscribeStreamContexts = new List<IRtmpSubscribeStreamContext> { subscribeStreamContext };
            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();
            var isSkippable = false;
            var payloadBuffer = _fixture.Create<DataBuffer>();

            var tcs = new TaskCompletionSource();

            subscriber.When(x => x.SendAsync(Arg.Any<IRentedBuffer>())).Do(_ => tcs.TrySetResult());

            // Act
            _sut.RegisterClient(subscriberContext);
            await _sut.BroadcastMediaMessageAsync(publishStreamContext, subscribeStreamContexts, mediaType, timestamp, isSkippable, payloadBuffer);

            // Assert
            await tcs.Task;

            Received.InOrder(() =>
            {
                subscribeStreamContext.Received(1).UntilInitializationComplete();
                subscriber.Received(1).SendAsync(Arg.Any<IRentedBuffer>());
            });
        }
    }
}
