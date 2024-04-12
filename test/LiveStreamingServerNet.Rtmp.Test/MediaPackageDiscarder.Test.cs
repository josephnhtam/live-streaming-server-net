using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.MediaPackageDiscarding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test
{
    public class MediaPackageDiscarderTest
    {
        private readonly uint _clientId;
        private readonly IOptions<MediaMessageConfiguration> _config;

        private readonly ILogger<MediaPackageDiscarder> _logger;

        public MediaPackageDiscarderTest()
        {
            _clientId = 123;

            _config = Options.Create(new MediaMessageConfiguration
            {
                MaxOutstandingMediaMessageSize = 100,
                MaxOutstandingMediaMessageCount = 10,
                TargetOutstandingMediaMessageSize = 50,
                TargetOutstandingMediaMessageCount = 5
            });

            _logger = Substitute.For<ILogger<MediaPackageDiscarder>>();
        }

        [Fact]
        public void ShouldDiscardMediaPackage_Should_ReturnFalse_When_NotDiscardable()
        {
            // Arrange
            var discarder = new MediaPackageDiscarder(_clientId, _config, _logger);

            // Act
            var result = discarder.ShouldDiscardMediaPackage(false, 200, 20);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void ShouldDiscardMediaPackage_Should_ReturnFalse_When_NotDiscarding()
        {
            // Arrange
            var discarder = new MediaPackageDiscarder(_clientId, _config, _logger);

            // Act
            var result = discarder.ShouldDiscardMediaPackage(true, 40, 4);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void ShouldDiscardMediaPackage_Should_ReturnFalse_When_WithinTargetRange()
        {
            // Arrange
            var discarder = new MediaPackageDiscarder(_clientId, _config, _logger);
            discarder.ShouldDiscardMediaPackage(true, 200, 20);

            // Act
            var result = discarder.ShouldDiscardMediaPackage(true, 40, 4);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void ShouldDiscardMediaPackage_Should_ReturnFalse_When_DiscardingAndAboveTargetRange()
        {
            // Arrange
            var discarder = new MediaPackageDiscarder(_clientId, _config, _logger);
            discarder.ShouldDiscardMediaPackage(true, 100, 100);

            // Act
            var result = discarder.ShouldDiscardMediaPackage(true, 70, 6);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public void ShouldDiscardMediaPackage_Should_ReturnTrue_When_AboveMaxThreshold()
        {
            // Arrange
            var discarder = new MediaPackageDiscarder(_clientId, _config, _logger);

            // Act
            var result = discarder.ShouldDiscardMediaPackage(true, 110, 11);

            // Assert
            result.Should().Be(true);
        }
    }
}
