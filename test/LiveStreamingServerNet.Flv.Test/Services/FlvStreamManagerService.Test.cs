using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test.Services
{
    public class FlvStreamManagerServiceTest
    {
        private readonly IFixture _fixture;
        private readonly FlvConfiguration _config;
        private readonly FlvStreamManagerService _sut;

        public FlvStreamManagerServiceTest()
        {
            _fixture = new Fixture();
            _config = new FlvConfiguration();
            _sut = new FlvStreamManagerService(Options.Create(_config));
        }

        [Fact]
        public void StartPublishingStream_Should_AddPublishingStreamAndReturnSucceeded()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);

            // Act
            var result = _sut.StartPublishingStream(streamContext);

            // Assert
            result.Should().Be(PublishingStreamResult.Succeeded);
            _sut.IsStreamPathPublishing(streamPath, false).Should().BeTrue();
        }

        [Fact]
        public void StartPublishingStream_Should_ReturnAlreadyExists_When_StreamIsAlreadyPublished()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamContext = Substitute.For<IFlvStreamContext>();
            streamContext.StreamPath.Returns(streamPath);

            _sut.StartPublishingStream(streamContext);

            // Act
            var result = _sut.StartPublishingStream(streamContext);

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

            _sut.StartPublishingStream(streamContext);

            // Act
            var result = _sut.StopPublishingStream(streamPath, false, out _);

            // Assert
            result.Should().BeTrue();
            _sut.IsStreamPathPublishing(streamPath, false).Should().BeFalse();
        }

        [Fact]
        public void StopPublishingStream_Should_ReturnFalse_When_StreamIsNotPublished()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();

            // Act
            var result = _sut.StopPublishingStream(streamPath, false, out _);

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

            _sut.StartPublishingStream(streamContext);

            // Act
            var result = _sut.IsStreamPathPublishing(streamPath, false);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsStreamPathPublishing_Should_ReturnFalse_When_StreamPathIsNotPublishing()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();

            // Act
            var result = _sut.IsStreamPathPublishing(streamPath, false);

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

            _sut.StartPublishingStream(streamContext);

            // Act
            var result = _sut.StartSubscribingStream(flvClient, streamPath, true);

            // Assert
            result.Should().Be(SubscribingStreamResult.Succeeded);
            _sut.GetSubscribers(streamPath).Should().Contain(flvClient);
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

            _sut.StartPublishingStream(streamContext);
            _sut.StartSubscribingStream(flvClient, streamPath, true);

            // Act
            var result = _sut.StartSubscribingStream(flvClient, streamPath, true);

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

            _sut.StartPublishingStream(streamContext);
            _sut.StartSubscribingStream(flvClient, streamPath, true);

            // Act
            var result = _sut.StopSubscribingStream(flvClient);

            // Assert
            result.Should().BeTrue();
            _sut.GetSubscribers(streamPath).Should().NotContain(flvClient);
        }

        [Fact]
        public void StopSubscribingStream_Should_ReturnFalse_When_SubscriberDoesNotExist()
        {
            // Arrange
            var flvClient = Substitute.For<IFlvClient>();

            // Act
            var result = _sut.StopSubscribingStream(flvClient);

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

            _sut.StartPublishingStream(streamContext);
            _sut.StartSubscribingStream(flvClient1, streamPath, true);
            _sut.StartSubscribingStream(flvClient2, streamPath, true);

            // Act
            var result = _sut.GetSubscribers(streamPath);

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

            _sut.StartPublishingStream(streamContext);
            _sut.StartSubscribingStream(flvClient1, streamPath, true);
            _sut.StartSubscribingStream(flvClient2, streamPath, true);

            // Act
            var result = _sut.StopPublishingStream(streamPath, false, out var existingSubscribers);

            // Assert
            existingSubscribers.Should().HaveCount(2);
            existingSubscribers.Should().Contain(flvClient1);
            existingSubscribers.Should().Contain(flvClient2);
        }
    }
}
