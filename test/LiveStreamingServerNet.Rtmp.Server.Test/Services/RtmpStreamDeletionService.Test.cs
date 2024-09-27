using AutoFixture;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
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
            var publisherContext = Substitute.For<IRtmpClientSessionContext>();
            var subscriberContext = Substitute.For<IRtmpClientSessionContext>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var publishStream = Substitute.For<IRtmpStream>();
            var subscribeStream = Substitute.For<IRtmpStream>();
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscribeStreamContexts = new List<IRtmpSubscribeStreamContext> { subscribeStreamContext };
            var streamPath = _fixture.Create<string>();
            var publishStreamId = _fixture.Create<uint>();
            var subscribeStreamId = _fixture.Create<uint>();

            publishStreamContext.StreamPath.Returns(streamPath);
            publishStreamContext.Stream.Returns(publishStream);
            publishStream.ClientContext.Returns(publisherContext);
            publishStream.PublishContext.Returns(publishStreamContext);
            publishStream.Id.Returns(publishStreamId);

            subscribeStreamContext.StreamPath.Returns(streamPath);
            subscribeStreamContext.Stream.Returns(subscribeStream);
            subscribeStream.ClientContext.Returns(subscriberContext);
            subscribeStream.SubscribeContext.Returns(subscribeStreamContext);
            subscribeStream.Id.Returns(subscribeStreamId);

            _rtmpStreamManager.StopPublishing(publishStreamContext, out Arg.Any<IList<IRtmpSubscribeStreamContext>>()).Returns(x =>
            {
                x[1] = subscribeStreamContexts;
                return true;
            });

            // Act
            await _rtmpStreamDeletionService.DeleteStreamAsync(publishStream);

            // Assert
            _userControlMessageSender.Received(1).SendStreamEofMessage(
                Arg.Is<IReadOnlyList<IRtmpSubscribeStreamContext>>(x => x.Contains(subscribeStreamContext)));

            _commandMessageSender.Received(1).SendCommandMessage(
                Arg.Is<IReadOnlyList<IRtmpClientSessionContext>>(x => x.Contains(subscriberContext)),
                subscribeStreamId,
                RtmpConstants.OnStatusChunkStreamId,
                "onStatus",
                0,
                null,
                Arg.Is<List<object?>>(x =>
                    x.First() is IDictionary<string, object> &&
                    (x.First() as IDictionary<string, object>)![RtmpArgumentNames.Level] as string == RtmpArgumentValues.Status &&
                    (x.First() as IDictionary<string, object>)![RtmpArgumentNames.Code] as string == RtmpStatusCodes.PlayUnpublishNotify
                ),
                Arg.Any<AmfEncodingType>()
            );

            _ = _eventDispatcher.Received(1).RtmpStreamUnpublishedAsync(publisherContext, streamPath);

            publishStream.Received(1).Delete();
        }

        [Fact]
        public async Task DeleteStream_Should_StopClientSubscribingStream_When_StreamIsBeingSubscribedByTheClient()
        {
            // Arrange
            var publisherContext = Substitute.For<IRtmpClientSessionContext>();
            var subscriberContext = Substitute.For<IRtmpClientSessionContext>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var publishStream = Substitute.For<IRtmpStream>();
            var subscribeStream = Substitute.For<IRtmpStream>();
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscribeStreamContexts = new List<IRtmpSubscribeStreamContext> { subscribeStreamContext };
            var streamPath = _fixture.Create<string>();
            var publishStreamId = _fixture.Create<uint>();
            var subscribeStreamId = _fixture.Create<uint>();

            publishStreamContext.StreamPath.Returns(streamPath);
            publishStreamContext.Stream.Returns(publishStream);
            publishStream.ClientContext.Returns(publisherContext);
            publishStream.PublishContext.Returns(publishStreamContext);
            publishStream.Id.Returns(publishStreamId);

            subscribeStreamContext.StreamPath.Returns(streamPath);
            subscribeStreamContext.Stream.Returns(subscribeStream);
            subscribeStream.ClientContext.Returns(subscriberContext);
            subscribeStream.SubscribeContext.Returns(subscribeStreamContext);
            subscribeStream.Id.Returns(subscribeStreamId);

            _rtmpStreamManager.StopSubscribing(subscribeStreamContext).Returns(true);

            // Act
            await _rtmpStreamDeletionService.DeleteStreamAsync(subscribeStream);

            // Assert
            _commandMessageSender.Received(1).SendCommandMessage(
                subscriberContext,
                subscribeStreamId,
                RtmpConstants.OnStatusChunkStreamId,
                "onStatus",
                0,
                null,
                Arg.Is<List<object?>>(x =>
                    x.First() is IDictionary<string, object> &&
                    (x.First() as IDictionary<string, object>)![RtmpArgumentNames.Level] as string == RtmpArgumentValues.Status &&
                    (x.First() as IDictionary<string, object>)![RtmpArgumentNames.Code] as string == RtmpStatusCodes.PlayUnpublishNotify
                ),
                Arg.Any<AmfEncodingType>()
            );

            _ = _eventDispatcher.Received(1).RtmpStreamUnsubscribedAsync(subscriberContext, streamPath);

            subscribeStream.Received(1).Delete();
        }
    }
}
