using AutoFixture;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.MediaPackageDiscarding.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
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
            var client = Substitute.For<IClientHandle>();

            var streamContext = Substitute.For<IRtmpStreamSubscriptionContext>();
            streamContext.IsReceivingVideo.Returns(true);
            streamContext.IsReceivingAudio.Returns(true);

            var clientContext = Substitute.For<IRtmpClientContext>();
            clientContext.Client.Returns(client);
            clientContext.StreamSubscriptionContext.Returns(streamContext);

            var publishStreamContext = new RtmpPublishStreamContext(
                _fixture.Create<uint>(),
                _fixture.Create<string>(),
                _fixture.Create<Dictionary<string, string>>(),
                null);

            var subscribers = new List<IRtmpClientContext> { clientContext };
            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();
            var isSkippable = false;
            var payloadBuffer = _fixture.Create<DataBuffer>();

            var tcs = new TaskCompletionSource();

            client.When(x => x.SendAsync(Arg.Any<IRentedBuffer>())).Do(_ => tcs.TrySetResult());

            // Act
            _sut.RegisterClient(clientContext);
            await _sut.BroadcastMediaMessageAsync(publishStreamContext, subscribers, mediaType, timestamp, isSkippable, payloadBuffer);

            // Assert
            await tcs.Task;

            Received.InOrder(() =>
            {
                streamContext.Received(1).UntilInitializationComplete();
                client.Received(1).SendAsync(Arg.Any<IRentedBuffer>());
            });
        }
    }
}
