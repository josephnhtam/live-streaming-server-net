using AutoFixture;
using LiveStreamingServerNet.Rtmp.Internal;
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
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly RtmpStreamDeletionService _rtmpStreamDeletionService;

        public RtmpStreamDeletionServiceTest()
        {
            _fixture = new Fixture();
            _rtmpStreamManager = Substitute.For<IRtmpStreamManagerService>();
            _userControlMessageSender = Substitute.For<IRtmpUserControlMessageSenderService>();
            _commandMessageSender = Substitute.For<IRtmpCommandMessageSenderService>();
            _eventDispatcher = Substitute.For<IRtmpServerStreamEventDispatcher>();

            _rtmpStreamDeletionService = new RtmpStreamDeletionService(
                _rtmpStreamManager,
                _userControlMessageSender,
                _commandMessageSender,
                _eventDispatcher);
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

            _rtmpStreamManager.StopPublishing(publisher_publishStreamContext, out Arg.Any<IList<IRtmpSubscribeStreamContext>>()).Returns(x =>
            {
                x[1] = subscriber_subscribeStreamContexts;
                return true;
            });

            // Act
            await _rtmpStreamDeletionService.DeleteStreamAsync(publisher_streamContext);

            // Assert
            Received.InOrder(() =>
            {
                _commandMessageSender.Received(1).SendCommandMessage(
                    Arg.Is<IReadOnlyList<IRtmpClientSessionContext>>(x => x.Contains(subscriber_clientContext)),
                    subscriber_streamId,
                    subscriber_CommandChunkStreamId,
                    "onStatus",
                    0,
                    null,
                    Arg.Is<List<object?>>(x =>
                        x.First() is IDictionary<string, object> &&
                        (x.First() as IDictionary<string, object>)![RtmpArguments.Level] as string == RtmpStatusLevels.Status &&
                        (x.First() as IDictionary<string, object>)![RtmpArguments.Code] as string == RtmpStreamStatusCodes.PlayUnpublishNotify
                    ),
                    Arg.Any<AmfEncodingType>()
                );

                _userControlMessageSender.Received(1).SendStreamEofMessage(
                    Arg.Is<IReadOnlyList<IRtmpSubscribeStreamContext>>(x => x.Contains(subscriber_subscribeStreamContext)));

                _ = _eventDispatcher.Received(1).RtmpStreamUnpublishedAsync(publisher_clientContext, streamPath);

                publisher_clientContext.Received(1).RemoveStreamContext(publisher_streamId);
            });
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

            _rtmpStreamManager.StopSubscribing(subscriber_subscribeStreamContext).Returns(true);

            // Act
            await _rtmpStreamDeletionService.DeleteStreamAsync(subscriber_streamContext);

            // Assert
            _ = _eventDispatcher.Received(1).RtmpStreamUnsubscribedAsync(subscriber_clientContext, streamPath);

            subscriber_clientContext.Received(1).RemoveStreamContext(subscriber_streamId);
        }
    }
}
