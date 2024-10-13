using AutoFixture;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Test.Utilities;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.Services
{
    public class RtmpStreamDeletionServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpStreamManagerService _rtmpStreamManager;
        private readonly RtmpStreamDeletionService _rtmpStreamDeletionService;

        public RtmpStreamDeletionServiceTest()
        {
            _fixture = new Fixture();
            _rtmpStreamManager = Substitute.For<IRtmpStreamManagerService>();

            _rtmpStreamDeletionService = new RtmpStreamDeletionService(_rtmpStreamManager);
        }

        [Fact]
        public async Task DeleteStream_Should_StopClientPublishingStream_When_StreamIsBeingPublishedByTheClient()
        {
            // Arrange
            var publisher_clientContext = Substitute.For<IRtmpClientSessionContext>();
            var subscriber_clientContext = Substitute.For<IRtmpClientSessionContext>();
            var publisher_publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var publisher_streamContext = Substitute.For<IRtmpStreamContext>();
            var subscriber_streamContext = Substitute.For<IRtmpStreamContext>();
            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscriber_subscribeStreamContexts = new List<IRtmpSubscribeStreamContext> { subscriber_subscribeStreamContext };
            var streamPath = _fixture.Create<string>();
            var publisher_streamId = _fixture.Create<uint>();
            var subscriber_streamId = _fixture.Create<uint>();
            var subscriber_CommandChunkStreamId = Helpers.CreateRandomChunkStreamId();

            publisher_publishStreamContext.StreamPath.Returns(streamPath);
            publisher_publishStreamContext.StreamContext.Returns(publisher_streamContext);
            publisher_streamContext.ClientContext.Returns(publisher_clientContext);
            publisher_streamContext.PublishContext.Returns(publisher_publishStreamContext);
            publisher_streamContext.StreamId.Returns(publisher_streamId);

            subscriber_subscribeStreamContext.StreamPath.Returns(streamPath);
            subscriber_subscribeStreamContext.StreamContext.Returns(subscriber_streamContext);
            subscriber_streamContext.ClientContext.Returns(subscriber_clientContext);
            subscriber_streamContext.SubscribeContext.Returns(subscriber_subscribeStreamContext);
            subscriber_streamContext.StreamId.Returns(subscriber_streamId);
            subscriber_streamContext.CommandChunkStreamId.Returns(subscriber_CommandChunkStreamId);

            _rtmpStreamManager.StopPublishingAsync(publisher_publishStreamContext)
                .Returns(x => (true, subscriber_subscribeStreamContexts));

            // Act
            await _rtmpStreamDeletionService.DeleteStreamAsync(publisher_streamContext);

            // Assert
            publisher_clientContext.Received(1).RemoveStreamContext(publisher_streamId);
        }

        [Fact]
        public async Task DeleteStream_Should_StopClientSubscribingStream_When_StreamIsBeingSubscribedByTheClient()
        {
            // Arrange
            var publisher_clientContext = Substitute.For<IRtmpClientSessionContext>();
            var subscriber_clientContext = Substitute.For<IRtmpClientSessionContext>();
            var publisher_publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var publisher_streamContext = Substitute.For<IRtmpStreamContext>();
            var subscriber_streamContext = Substitute.For<IRtmpStreamContext>();
            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscriber_subscribeStreamContexts = new List<IRtmpSubscribeStreamContext> { subscriber_subscribeStreamContext };
            var streamPath = _fixture.Create<string>();
            var publisher_streamId = _fixture.Create<uint>();
            var subscriber_streamId = _fixture.Create<uint>();

            publisher_publishStreamContext.StreamPath.Returns(streamPath);
            publisher_publishStreamContext.StreamContext.Returns(publisher_streamContext);
            publisher_streamContext.ClientContext.Returns(publisher_clientContext);
            publisher_streamContext.PublishContext.Returns(publisher_publishStreamContext);
            publisher_streamContext.StreamId.Returns(publisher_streamId);

            subscriber_subscribeStreamContext.StreamPath.Returns(streamPath);
            subscriber_subscribeStreamContext.StreamContext.Returns(subscriber_streamContext);
            subscriber_streamContext.ClientContext.Returns(subscriber_clientContext);
            subscriber_streamContext.SubscribeContext.Returns(subscriber_subscribeStreamContext);
            subscriber_streamContext.StreamId.Returns(subscriber_streamId);

            _rtmpStreamManager.StopSubscribingAsync(subscriber_subscribeStreamContext).Returns(true);

            // Act
            await _rtmpStreamDeletionService.DeleteStreamAsync(subscriber_streamContext);

            // Assert
            subscriber_clientContext.Received(1).RemoveStreamContext(subscriber_streamId);
        }
    }
}
