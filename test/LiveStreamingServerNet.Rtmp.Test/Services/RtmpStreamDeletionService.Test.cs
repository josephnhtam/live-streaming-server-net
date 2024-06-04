using AutoFixture;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
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
            var clientContext = Substitute.For<IRtmpClientContext>();
            var existingSubscriber = Substitute.For<IRtmpClientContext>();
            var existingSubscribers = new List<IRtmpClientContext> { existingSubscriber };

            var streamPath = _fixture.Create<string>();
            clientContext.PublishStreamContext!.StreamPath.Returns(streamPath);

            _rtmpStreamManager.StopPublishingStream(clientContext, out Arg.Any<IList<IRtmpClientContext>>()).Returns(x =>
            {
                x[1] = existingSubscribers;
                return true;
            });

            // Act
            await _rtmpStreamDeletionService.DeleteStreamAsync(clientContext);

            // Assert
            _userControlMessageSender.Received(1).SendStreamEofMessage(
                Arg.Is<IReadOnlyList<IRtmpClientContext>>(x => x.Contains(existingSubscriber)));

            _commandMessageSender.Received(1).SendCommandMessage(
                Arg.Is<IReadOnlyList<IRtmpClientContext>>(x => x.Contains(existingSubscriber)),
                Arg.Any<uint>(),
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

            _ = _eventDispatcher.Received(1).RtmpStreamUnpublishedAsync(clientContext, streamPath);

            clientContext.Received(1).DeleteStream();
        }

        [Fact]
        public async Task DeleteStream_Should_StopClientSubscribingStream_When_StreamIsBeingSubscribedByTheClient()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientContext>();
            var existingSubscriber = Substitute.For<IRtmpClientContext>();
            var existingSubscribers = new List<IRtmpClientContext> { existingSubscriber };

            var streamPath = _fixture.Create<string>();
            clientContext.StreamSubscriptionContext!.StreamPath.Returns(streamPath);

            _rtmpStreamManager.StopSubscribingStream(clientContext).Returns(true);

            // Act
            await _rtmpStreamDeletionService.DeleteStreamAsync(clientContext);

            // Assert
            _commandMessageSender.Received(1).SendCommandMessage(
                clientContext,
                Arg.Any<uint>(),
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

            _eventDispatcher.Received(1).RtmpStreamUnsubscribedAsync(clientContext, streamPath);

            clientContext.Received(1).DeleteStream();
        }
    }
}
