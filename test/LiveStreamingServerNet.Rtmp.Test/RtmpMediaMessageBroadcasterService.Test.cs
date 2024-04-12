using AutoFixture;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit.Abstractions;

namespace LiveStreamingServerNet.Rtmp.Test
{
    public class RtmpMediaMessageBroadcasterServiceTest
    {
        private readonly Fixture _fixture;
        private readonly IRtmpChunkMessageWriterService _chunkMessageWriter;
        private readonly IRtmpMediaMessageInterceptionService _interception;
        private readonly NetBufferPool _netBufferPool;
        private readonly IOptions<MediaMessageConfiguration> _config;
        private readonly ILogger<RtmpMediaMessageBroadcasterService> _logger;
        private readonly RtmpMediaMessageBroadcasterService _sut;
        private readonly ITestOutputHelper _output;

        public RtmpMediaMessageBroadcasterServiceTest(ITestOutputHelper output)
        {
            _fixture = new Fixture();
            _chunkMessageWriter = Substitute.For<IRtmpChunkMessageWriterService>();
            _interception = Substitute.For<IRtmpMediaMessageInterceptionService>();
            _netBufferPool = new NetBufferPool(Options.Create(new NetBufferPoolConfiguration()));
            _config = Options.Create(new MediaMessageConfiguration());
            _logger = Substitute.For<ILogger<RtmpMediaMessageBroadcasterService>>();

            _sut = new RtmpMediaMessageBroadcasterService(_chunkMessageWriter, _interception, _netBufferPool, _config, _logger);
            _output = output;
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
            client.When(x => x.SendAsync(Arg.Any<IRentedBuffer>())).Do(_ =>
            {
                if (Interlocked.Increment(ref sendCount) == 2)
                    tcs.TrySetResult();
            });

            // Act
            _sut.RegisterClient(clientContext);
            await _sut.BroadcastMediaMessageAsync(publishStreamContext, subscribers, mediaType, timestamp, isSkippable, payloadBuffer);
            await _sut.BroadcastMediaMessageAsync(publishStreamContext, subscribers, mediaType, timestamp, isSkippable, payloadBuffer);

            // Assert
            await tcs.Task;

            Received.InOrder(() =>
            {
                streamContext.Received(1).UntilInitializationComplete();
                client.Received(1).SendAsync(Arg.Any<IRentedBuffer>());
                client.Received(1).SendAsync(Arg.Any<IRentedBuffer>());
            });
        }
    }
}
