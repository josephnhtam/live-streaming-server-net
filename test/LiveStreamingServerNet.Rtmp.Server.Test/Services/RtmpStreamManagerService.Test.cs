using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.Services
{
    public class RtmpStreamManagerServiceTest
    {
        private readonly IFixture _fixture;

        public RtmpStreamManagerServiceTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void GetPublishStreamContext_Should_ReturnCorrectStreamContext()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            var streamContext = clientContext.CreateStreamContext();
            sut.StartPublishing(streamContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.GetPublishStreamContext(streamPath);

            // Assert
            result.Should().Be(streamContext.PublishContext);
        }

        [Fact]
        public void StartPublishing_Should_AddPublisherAndReturnSucceeded()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            streamContext.ClientContext.Returns(clientContext);
            streamContext.PublishContext.Returns((IRtmpPublishStreamContext?)null);
            streamContext.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);

            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StartPublishing(streamContext, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.Succeeded);
            streamContext.Received(1).CreatePublishContext(streamPath, streamArguments);
            sut.IsStreamPublishing(streamPath).Should().BeTrue();
        }

        [Fact]
        public void StartPublishing_Should_ReturnAlreadySubscribing_When_PublisherIsAlreadySubscribing()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            streamContext.ClientContext.Returns(clientContext);
            streamContext.PublishContext.Returns((IRtmpPublishStreamContext?)null);
            streamContext.SubscribeContext.Returns(subscribeStreamContext);
            subscribeStreamContext.StreamContext.Returns(streamContext);

            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StartPublishing(streamContext, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.AlreadySubscribing);
        }

        [Fact]
        public void StartPublishing_Should_ReturnAlreadyPublishing_When_PublisherIsAlreadyPublishing()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            streamContext.ClientContext.Returns(clientContext);
            streamContext.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);
            streamContext.PublishContext.Returns(publishStreamContext);
            publishStreamContext.StreamContext.Returns(streamContext);

            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StartPublishing(streamContext, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.AlreadyPublishing);
        }

        [Fact]
        public void StartPublishing_Should_ReturnAlreadyExists_When_StreamPathAlreadyExists()
        {
            // Arrange
            var sessionHandle1 = Substitute.For<ISessionHandle>();
            var clientContext1 = new RtmpClientSessionContext(sessionHandle1, null);
            var streamContext1 = clientContext1.CreateStreamContext();

            var sessionHandle2 = Substitute.For<ISessionHandle>();
            var clientContext2 = new RtmpClientSessionContext(sessionHandle2, null);
            var streamContext2 = clientContext2.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartPublishing(streamContext1, streamPath, streamArguments, out _);

            // Act
            var result = sut.StartPublishing(streamContext2, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.AlreadyExists);
        }

        [Fact]
        public void StopPublishing_Should_RemovePublisherAndReturnTrue()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartPublishing(streamContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.StopPublishing(streamContext.PublishContext!, out _);

            // Assert
            result.Should().BeTrue();
            streamContext.PublishContext.Should().BeNull();
            sut.IsStreamPublishing(streamPath).Should().BeFalse();
        }

        [Fact]
        public void StopPublishing_Should_ReturnFalse_When_PublisherDoesNotExist()
        {
            // Arrange
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StopPublishing(publishStreamContext, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsStreamPathPublishing_Should_ReturnTrue_When_StreamPathIsPublishing()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartPublishing(streamContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.IsStreamPublishing(streamPath);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsStreamPathPublishing_Should_ReturnFalse_When_StreamPathIsNotPublishing()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.IsStreamPublishing(streamPath);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void StartSubscribing_Should_AddSubscriberAndReturnSucceeded()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StartSubscribing(streamContext, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(SubscribingStreamResult.Succeeded);
            sut.GetSubscribeStreamContexts(streamPath).Should().Contain(streamContext.SubscribeContext!);
        }

        [Fact]
        public void StartSubscribing_Should_ReturnAlreadyPublishing_When_SubscriberIsAlreadyPublishing()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartPublishing(streamContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.StartSubscribing(streamContext, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(SubscribingStreamResult.AlreadyPublishing);
        }

        [Fact]
        public void StartSubscribing_Should_ReturnAlreadySubscribing_When_SubscriberIsAlreadySubscribing()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartSubscribing(streamContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.StartSubscribing(streamContext, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(SubscribingStreamResult.AlreadySubscribing);
        }

        [Fact]
        public void StopSubscribing_Should_RemoveSubscriberAndReturnTrue()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var streamContext = clientContext.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartSubscribing(streamContext, streamPath, streamArguments, out _);

            var subscribeStreamContext = streamContext.SubscribeContext!;

            // Act
            var result = sut.StopSubscribing(subscribeStreamContext);

            // Assert
            result.Should().BeTrue();
            sut.GetSubscribeStreamContexts(streamPath).Should().NotContain(subscribeStreamContext);
        }

        [Fact]
        public void StopSubscribing_Should_ReturnFalse_When_SubscriberDoesNotExist()
        {
            // Arrange
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StopSubscribing(subscribeStreamContext);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetSubscribeStreamContexts_Should_ReturnCorrectSubscriberStreamContexts()
        {
            // Arrange
            var sessionHandle1 = Substitute.For<ISessionHandle>();
            var clientContext1 = new RtmpClientSessionContext(sessionHandle1, null);
            var streamContext1 = clientContext1.CreateStreamContext();

            var sessionHandle2 = Substitute.For<ISessionHandle>();
            var clientContext2 = new RtmpClientSessionContext(sessionHandle2, null);
            var streamContext2 = clientContext2.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartSubscribing(streamContext1, streamPath, streamArguments, out _);
            sut.StartSubscribing(streamContext2, streamPath, streamArguments, out _);

            // Act
            var result = sut.GetSubscribeStreamContexts(streamPath);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(streamContext1.SubscribeContext!);
            result.Should().Contain(streamContext2.SubscribeContext!);
        }

        [Fact]
        public void StartPublishing_Should_ReturnCorrectSubscribers()
        {
            // Arrange
            var sessionHandle1 = Substitute.For<ISessionHandle>();
            var clientContext1 = new RtmpClientSessionContext(sessionHandle1, null);
            var streamContext1 = clientContext1.CreateStreamContext();

            var sessionHandle2 = Substitute.For<ISessionHandle>();
            var clientContext2 = new RtmpClientSessionContext(sessionHandle2, null);
            var streamContext2 = clientContext2.CreateStreamContext();

            var sessionHandle3 = Substitute.For<ISessionHandle>();
            var clientContext3 = new RtmpClientSessionContext(sessionHandle3, null);
            var streamContext3 = clientContext2.CreateStreamContext();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartSubscribing(streamContext1, streamPath, streamArguments, out _);
            sut.StartSubscribing(streamContext2, streamPath, streamArguments, out _);

            // Act
            var result = sut.StartPublishing(streamContext3, streamPath, streamArguments, out var existingSubscribers);

            // Assert
            result.Should().Be(PublishingStreamResult.Succeeded);
            sut.IsStreamPublishing(streamPath).Should().BeTrue();
            existingSubscribers.Should().HaveCount(2);
            existingSubscribers.Should().Contain(streamContext1.SubscribeContext!);
            existingSubscribers.Should().Contain(streamContext2.SubscribeContext!);
        }
    }
}
