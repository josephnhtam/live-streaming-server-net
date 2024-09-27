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
            var stream = clientContext.CreateNewStream();
            sut.StartPublishing(stream, streamPath, streamArguments, out _);

            // Act
            var result = sut.GetPublishStreamContext(streamPath);

            // Assert
            result.Should().Be(stream.PublishContext);
        }

        [Fact]
        public void StartPublishing_Should_AddPublisherAndReturnSucceeded()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var stream = Substitute.For<IRtmpStream>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            stream.ClientContext.Returns(clientContext);
            stream.PublishContext.Returns((IRtmpPublishStreamContext?)null);
            stream.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);

            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StartPublishing(stream, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.Succeeded);
            stream.Received(1).CreatePublishContext(streamPath, streamArguments);
            sut.IsStreamPublishing(streamPath).Should().BeTrue();
        }

        [Fact]
        public void StartPublishing_Should_ReturnAlreadySubscribing_When_PublisherIsAlreadySubscribing()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var subscribeStream = Substitute.For<IRtmpStream>();
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            subscribeStream.ClientContext.Returns(clientContext);
            subscribeStream.PublishContext.Returns((IRtmpPublishStreamContext?)null);
            subscribeStream.SubscribeContext.Returns(subscribeStreamContext);
            subscribeStreamContext.Stream.Returns(subscribeStream);

            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StartPublishing(subscribeStream, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.AlreadySubscribing);
        }

        [Fact]
        public void StartPublishing_Should_ReturnAlreadyPublishing_When_PublisherIsAlreadyPublishing()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var stream = Substitute.For<IRtmpStream>();
            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            stream.ClientContext.Returns(clientContext);
            stream.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);
            stream.PublishContext.Returns(publishStreamContext);
            publishStreamContext.Stream.Returns(stream);

            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StartPublishing(stream, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.AlreadyPublishing);
        }

        [Fact]
        public void StartPublishing_Should_ReturnAlreadyExists_When_StreamPathAlreadyExists()
        {
            // Arrange
            var sessionHandle1 = Substitute.For<ISessionHandle>();
            var clientContext1 = new RtmpClientSessionContext(sessionHandle1, null);
            var stream1 = clientContext1.CreateNewStream();

            var sessionHandle2 = Substitute.For<ISessionHandle>();
            var clientContext2 = new RtmpClientSessionContext(sessionHandle2, null);
            var stream2 = clientContext2.CreateNewStream();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartPublishing(stream1, streamPath, streamArguments, out _);

            // Act
            var result = sut.StartPublishing(stream2, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.AlreadyExists);
        }

        [Fact]
        public void StopPublishing_Should_RemovePublisherAndReturnTrue()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var stream = clientContext.CreateNewStream();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartPublishing(stream, streamPath, streamArguments, out _);

            // Act
            var result = sut.StopPublishing(stream.PublishContext!, out _);

            // Assert
            result.Should().BeTrue();
            stream.PublishContext.Should().BeNull();
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
            var stream = clientContext.CreateNewStream();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartPublishing(stream, streamPath, streamArguments, out _);

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
            var stream = clientContext.CreateNewStream();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StartSubscribing(stream, streamPath, streamArguments);

            // Assert
            result.Should().Be(SubscribingStreamResult.Succeeded);
            sut.GetSubscribeStreamContexts(streamPath).Should().Contain(stream.SubscribeContext!);
        }

        [Fact]
        public void StartSubscribing_Should_ReturnAlreadyPublishing_When_SubscriberIsAlreadyPublishing()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var stream = clientContext.CreateNewStream();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartPublishing(stream, streamPath, streamArguments, out _);

            // Act
            var result = sut.StartSubscribing(stream, streamPath, streamArguments);

            // Assert
            result.Should().Be(SubscribingStreamResult.AlreadyPublishing);
        }

        [Fact]
        public void StartSubscribing_Should_ReturnAlreadySubscribing_When_SubscriberIsAlreadySubscribing()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var stream = clientContext.CreateNewStream();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartSubscribing(stream, streamPath, streamArguments);

            // Act
            var result = sut.StartSubscribing(stream, streamPath, streamArguments);

            // Assert
            result.Should().Be(SubscribingStreamResult.AlreadySubscribing);
        }

        [Fact]
        public void StopSubscribing_Should_RemoveSubscriberAndReturnTrue()
        {
            // Arrange
            var sessionHandle = Substitute.For<ISessionHandle>();
            var clientContext = new RtmpClientSessionContext(sessionHandle, null);
            var stream = clientContext.CreateNewStream();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartSubscribing(stream, streamPath, streamArguments);

            var subscribeStreamContext = stream.SubscribeContext!;

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
            var stream1 = clientContext1.CreateNewStream();

            var sessionHandle2 = Substitute.For<ISessionHandle>();
            var clientContext2 = new RtmpClientSessionContext(sessionHandle2, null);
            var stream2 = clientContext2.CreateNewStream();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartSubscribing(stream1, streamPath, streamArguments);
            sut.StartSubscribing(stream2, streamPath, streamArguments);

            // Act
            var result = sut.GetSubscribeStreamContexts(streamPath);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(stream1.SubscribeContext!);
            result.Should().Contain(stream2.SubscribeContext!);
        }

        [Fact]
        public void StartPublishing_Should_ReturnCorrectSubscribers()
        {
            // Arrange
            var sessionHandle1 = Substitute.For<ISessionHandle>();
            var clientContext1 = new RtmpClientSessionContext(sessionHandle1, null);
            var stream1 = clientContext1.CreateNewStream();

            var sessionHandle2 = Substitute.For<ISessionHandle>();
            var clientContext2 = new RtmpClientSessionContext(sessionHandle2, null);
            var stream2 = clientContext2.CreateNewStream();

            var sessionHandle3 = Substitute.For<ISessionHandle>();
            var clientContext3 = new RtmpClientSessionContext(sessionHandle3, null);
            var stream3 = clientContext2.CreateNewStream();

            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();

            var sut = new RtmpStreamManagerService();
            sut.StartSubscribing(stream1, streamPath, streamArguments);
            sut.StartSubscribing(stream2, streamPath, streamArguments);

            // Act
            var result = sut.StartPublishing(stream3, streamPath, streamArguments, out var existingSubscribers);

            // Assert
            result.Should().Be(PublishingStreamResult.Succeeded);
            sut.IsStreamPublishing(streamPath).Should().BeTrue();
            existingSubscribers.Should().HaveCount(2);
            existingSubscribers.Should().Contain(stream1.SubscribeContext!);
            existingSubscribers.Should().Contain(stream2.SubscribeContext!);
        }
    }
}
