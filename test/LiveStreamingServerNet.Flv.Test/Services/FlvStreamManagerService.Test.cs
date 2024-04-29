using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test.Services
{
    public class FlvStreamManagerServiceTest
    {
        private readonly IFixture _fixture;

        public FlvStreamManagerServiceTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void StartPublishingStream_Should_AddPublishingStreamAndReturnSucceeded()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);

            var sut = new FlvStreamManagerService();

            // Act
            var result = sut.StartPublishingStream(streamContext);

            // Assert
            result.Should().Be(PublishingStreamResult.Succeeded);
            sut.IsStreamPathPublishing(streamPath, false).Should().BeTrue();
        }

        [Fact]
        public void StartPublishingStream_Should_ReturnAlreadyExists_When_StreamIsAlreadyPublished()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);

            var sut = new FlvStreamManagerService();
            sut.StartPublishingStream(streamContext);

            // Act
            var result = sut.StartPublishingStream(streamContext);

            // Assert
            result.Should().Be(PublishingStreamResult.AlreadyExists);
        }

        [Fact]
        public void StopPublishingStream_Should_RemovePublishingStreamAndReturnTrue()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);

            var sut = new FlvStreamManagerService();
            sut.StartPublishingStream(streamContext);

            // Act
            var result = sut.StopPublishingStream(streamPath, out _);

            // Assert
            result.Should().BeTrue();
            sut.IsStreamPathPublishing(streamPath, false).Should().BeFalse();
        }

        [Fact]
        public void StopPublishingStream_Should_ReturnFalse_When_StreamIsNotPublished()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var sut = new FlvStreamManagerService();

            // Act
            var result = sut.StopPublishingStream(streamPath, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsStreamPathPublishing_Should_ReturnTrue_When_StreamPathIsPublishing()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);

            var sut = new FlvStreamManagerService();
            sut.StartPublishingStream(streamContext);

            // Act
            var result = sut.IsStreamPathPublishing(streamPath, false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsStreamPathPublishing_Should_ReturnFalse_When_StreamPathIsNotPublishing()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var sut = new FlvStreamManagerService();

            // Act
            var result = sut.IsStreamPathPublishing(streamPath, false);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void StartSubscribingStream_Should_AddSubscriberAndReturnSucceeded()
        {
            // Arrange
            var flvClient = Substitute.For<IFlvClient>();
            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);
            streamContext.IsReady.Returns(true);

            var sut = new FlvStreamManagerService();
            sut.StartPublishingStream(streamContext);

            // Act
            var result = sut.StartSubscribingStream(flvClient, streamPath);

            // Assert
            result.Should().Be(SubscribingStreamResult.Succeeded);
            sut.GetSubscribers(streamPath).Should().Contain(flvClient);
        }

        [Fact]
        public void StartSubscribingStream_Should_ReturnAlreadySubscribing_When_SubscriberIsAlreadySubscribing()
        {
            // Arrange
            var flvClient = Substitute.For<IFlvClient>();
            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);
            streamContext.IsReady.Returns(true);

            var sut = new FlvStreamManagerService();
            sut.StartPublishingStream(streamContext);
            sut.StartSubscribingStream(flvClient, streamPath);

            // Act
            var result = sut.StartSubscribingStream(flvClient, streamPath);

            // Assert
            result.Should().Be(SubscribingStreamResult.AlreadySubscribing);
        }

        [Fact]
        public void StopSubscribingStream_Should_RemoveSubscriberAndReturnTrue()
        {
            // Arrange
            var flvClient = Substitute.For<IFlvClient>();
            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);
            streamContext.IsReady.Returns(true);

            var sut = new FlvStreamManagerService();
            sut.StartPublishingStream(streamContext);
            sut.StartSubscribingStream(flvClient, streamPath);

            // Act
            var result = sut.StopSubscribingStream(flvClient);

            // Assert
            result.Should().BeTrue();
            sut.GetSubscribers(streamPath).Should().NotContain(flvClient);
        }

        [Fact]
        public void StopSubscribingStream_Should_ReturnFalse_When_SubscriberDoesNotExist()
        {
            // Arrange
            var flvClient = Substitute.For<IFlvClient>();
            var sut = new FlvStreamManagerService();

            // Act
            var result = sut.StopSubscribingStream(flvClient);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetSubscribers_Should_ReturnCorrectSubscribers()
        {
            // Arrange
            var flvClient1 = Substitute.For<IFlvClient>();
            var flvClient2 = Substitute.For<IFlvClient>();

            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);
            streamContext.IsReady.Returns(true);

            var sut = new FlvStreamManagerService();
            sut.StartPublishingStream(streamContext);
            sut.StartSubscribingStream(flvClient1, streamPath);
            sut.StartSubscribingStream(flvClient2, streamPath);

            // Act
            var result = sut.GetSubscribers(streamPath);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(flvClient1);
            result.Should().Contain(flvClient2);
        }

        [Fact]
        public void StartPublishingStream_Should_ReturnCorrectSubscribers()
        {
            // Arrange
            var flvClient1 = Substitute.For<IFlvClient>();
            var flvClient2 = Substitute.For<IFlvClient>();

            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);
            streamContext.IsReady.Returns(true);

            var sut = new FlvStreamManagerService();
            sut.StartPublishingStream(streamContext);
            sut.StartSubscribingStream(flvClient1, streamPath);
            sut.StartSubscribingStream(flvClient2, streamPath);

            // Act
            var result = sut.StopPublishingStream(streamPath, out var existingSubscribers);

            // Assert
            existingSubscribers.Should().HaveCount(2);
            existingSubscribers.Should().Contain(flvClient1);
            existingSubscribers.Should().Contain(flvClient2);
        }
    }
}
