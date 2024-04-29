using AutoFixture;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.MediaPackageDiscarding.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
{
    public class RtmpMediaMessageBroadcasterServiceTest
    {
        private readonly Fixture _fixture;
        private readonly IRtmpChunkMessageWriterService _chunkMessageWriter;
        private readonly IRtmpMediaMessageInterceptionService _interception;
        private readonly INetBufferPool _netBufferPool;
        private readonly IMediaPackageDiscarder _mediaPackageDiscarder;
        private readonly IMediaPackageDiscarderFactory _mediaPackageDiscarderFactory;
        private readonly ILogger<RtmpMediaMessageBroadcasterService> _logger;
        private readonly IRtmpMediaMessageBroadcasterService _sut;

        public RtmpMediaMessageBroadcasterServiceTest()
        {
            _fixture = new Fixture();
            _chunkMessageWriter = Substitute.For<IRtmpChunkMessageWriterService>();
            _interception = Substitute.For<IRtmpMediaMessageInterceptionService>();
            _netBufferPool = new NetBufferPool(Options.Create(new NetBufferPoolConfiguration()));
            _mediaPackageDiscarder = Substitute.For<IMediaPackageDiscarder>();
            _mediaPackageDiscarderFactory = Substitute.For<IMediaPackageDiscarderFactory>();
            _logger = Substitute.For<ILogger<RtmpMediaMessageBroadcasterService>>();

            _mediaPackageDiscarderFactory.Create(Arg.Any<uint>()).Returns(_mediaPackageDiscarder);
            _mediaPackageDiscarder.ShouldDiscardMediaPackage(Arg.Any<bool>(), Arg.Any<long>(), Arg.Any<long>())
                .Returns(false);

            _sut = new RtmpMediaMessageBroadcasterService(_chunkMessageWriter, _interception, _netBufferPool, _mediaPackageDiscarderFactory, _logger);
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

            var publishStreamContext = _fixture.Create<RtmpPublishStreamContext>();
            var subscribers = new List<IRtmpClientContext> { clientContext };
            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();
            var isSkippable = false;
            var payloadBuffer = _fixture.Create<NetBuffer>();

            var tcs = new TaskCompletionSource();

            int sendCount = 0;
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
