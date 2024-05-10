using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
{
    public class RtmpStreamManagerServiceTest
    {
        private readonly IFixture _fixture;

        public RtmpStreamManagerServiceTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void GetPublishStreamPath_Should_ReturnCorrectStreamPath()
        {
            // Arrange
            var publisherClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartPublishingStream(publisherClientContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.GetPublishStreamPath(publisherClientContext);

            // Assert
            result.Should().Be(streamPath);
        }

        [Fact]
        public void GetPublishingClientContext_Should_ReturnCorrectClientContext()
        {
            // Arrange
            var publisherClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartPublishingStream(publisherClientContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.GetPublishingClientContext(streamPath);

            // Assert
            result.Should().Be(publisherClientContext);
        }

        [Fact]
        public void GetPublishStreamContext_Should_ReturnCorrectStreamContext()
        {
            // Arrange
            var publisherClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartPublishingStream(publisherClientContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.GetPublishStreamContext(streamPath);

            // Assert
            result.Should().Be(publisherClientContext.PublishStreamContext);
        }

        [Fact]
        public void StartPublishingStream_Should_AddPublisherAndReturnSucceeded()
        {
            // Arrange
            var publisherClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StartPublishingStream(publisherClientContext, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.Succeeded);
            sut.IsStreamPathPublishing(streamPath).Should().BeTrue();
        }

        [Fact]
        public void StartPublishingStream_Should_ReturnAlreadySubscribing_When_PublisherIsAlreadySubscribing()
        {
            // Arrange
            var publisherClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartSubscribingStream(publisherClientContext, 1, streamPath, streamArguments);

            // Act
            var result = sut.StartPublishingStream(publisherClientContext, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.AlreadySubscribing);
        }

        [Fact]
        public void StartPublishingStream_Should_ReturnAlreadyPublishing_When_PublisherIsAlreadyPublishing()
        {
            // Arrange
            var publisherClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartPublishingStream(publisherClientContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.StartPublishingStream(publisherClientContext, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.AlreadyPublishing);
        }

        [Fact]
        public void StartPublishingStream_Should_ReturnAlreadyExists_When_StreamPathAlreadyExists()
        {
            // Arrange
            var publisherClientContext1 = Substitute.For<IRtmpClientContext>();
            var publisherClientContext2 = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartPublishingStream(publisherClientContext1, streamPath, streamArguments, out _);

            // Act
            var result = sut.StartPublishingStream(publisherClientContext2, streamPath, streamArguments, out _);

            // Assert
            result.Should().Be(PublishingStreamResult.AlreadyExists);
        }

        [Fact]
        public void StopPublishingStream_Should_RemovePublisherAndReturnTrue()
        {
            // Arrange
            var publisherClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartPublishingStream(publisherClientContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.StopPublishingStream(publisherClientContext, out _);

            // Assert
            result.Should().BeTrue();
            sut.IsStreamPathPublishing(streamPath).Should().BeFalse();
        }

        [Fact]
        public void StopPublishingStream_Should_ReturnFalse_When_PublisherDoesNotExist()
        {
            // Arrange
            var publisherClientContext = Substitute.For<IRtmpClientContext>();
            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StopPublishingStream(publisherClientContext, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsStreamPathPublishing_Should_ReturnTrue_When_StreamPathIsPublishing()
        {
            // Arrange
            var publisherClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartPublishingStream(publisherClientContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.IsStreamPathPublishing(streamPath);

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
            var result = sut.IsStreamPathPublishing(streamPath);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void StartSubscribingStream_Should_AddSubscriberAndReturnSucceeded()
        {
            // Arrange
            var subscriberClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StartSubscribingStream(subscriberClientContext, 1, streamPath, streamArguments);

            // Assert
            result.Should().Be(SubscribingStreamResult.Succeeded);
            sut.GetSubscribers(streamPath).Should().Contain(subscriberClientContext);
        }

        [Fact]
        public void StartSubscribingStream_Should_ReturnAlreadyPublishing_When_SubscriberIsAlreadyPublishing()
        {
            // Arrange
            var subscriberClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartPublishingStream(subscriberClientContext, streamPath, streamArguments, out _);

            // Act
            var result = sut.StartSubscribingStream(subscriberClientContext, 1, streamPath, streamArguments);

            // Assert
            result.Should().Be(SubscribingStreamResult.AlreadyPublishing);
        }

        [Fact]
        public void StartSubscribingStream_Should_ReturnAlreadySubscribing_When_SubscriberIsAlreadySubscribing()
        {
            // Arrange
            var subscriberClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartSubscribingStream(subscriberClientContext, 1, streamPath, streamArguments);

            // Act
            var result = sut.StartSubscribingStream(subscriberClientContext, 1, streamPath, streamArguments);

            // Assert
            result.Should().Be(SubscribingStreamResult.AlreadySubscribing);
        }

        [Fact]
        public void StopSubscribingStream_Should_RemoveSubscriberAndReturnTrue()
        {
            // Arrange
            var subscriberClientContext = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartSubscribingStream(subscriberClientContext, 1, streamPath, streamArguments);

            // Act
            var result = sut.StopSubscribingStream(subscriberClientContext);

            // Assert
            result.Should().BeTrue();
            sut.GetSubscribers(streamPath).Should().NotContain(subscriberClientContext);
        }

        [Fact]
        public void StopSubscribingStream_Should_ReturnFalse_When_SubscriberDoesNotExist()
        {
            // Arrange
            var subscriberClientContext = Substitute.For<IRtmpClientContext>();
            var sut = new RtmpStreamManagerService();

            // Act
            var result = sut.StopSubscribingStream(subscriberClientContext);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetSubscribers_Should_ReturnCorrectSubscribers()
        {
            // Arrange
            var subscriberClientContext1 = Substitute.For<IRtmpClientContext>();
            var subscriberClientContext2 = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartSubscribingStream(subscriberClientContext1, 1, streamPath, streamArguments);
            sut.StartSubscribingStream(subscriberClientContext2, 2, streamPath, streamArguments);

            // Act
            var result = sut.GetSubscribers(streamPath);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(subscriberClientContext1);
            result.Should().Contain(subscriberClientContext2);
        }

        [Fact]
        public void StartPublishingStream_Should_ReturnCorrectSubscribers()
        {
            // Arrange
            var publisherClientContext = Substitute.For<IRtmpClientContext>();
            var subscriberClientContext1 = Substitute.For<IRtmpClientContext>();
            var subscriberClientContext2 = Substitute.For<IRtmpClientContext>();
            var streamPath = _fixture.Create<string>();
            var streamArguments = _fixture.Create<Dictionary<string, string>>();
            var sut = new RtmpStreamManagerService();
            sut.StartSubscribingStream(subscriberClientContext1, 1, streamPath, streamArguments);
            sut.StartSubscribingStream(subscriberClientContext2, 2, streamPath, streamArguments);

            // Act
            var result = sut.StartPublishingStream(publisherClientContext, streamPath, streamArguments, out var existingSubscribers);

            // Assert
            result.Should().Be(PublishingStreamResult.Succeeded);
            sut.IsStreamPathPublishing(streamPath).Should().BeTrue();
            existingSubscribers.Should().HaveCount(2);
            existingSubscribers.Should().Contain(subscriberClientContext1);
            existingSubscribers.Should().Contain(subscriberClientContext2);
        }
    }
}
