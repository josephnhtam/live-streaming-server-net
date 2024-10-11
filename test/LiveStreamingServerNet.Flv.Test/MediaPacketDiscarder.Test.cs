using FluentAssertions;
using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test
{
    public class MediaPacketDiscarderTest
    {
        private readonly string _clientId;
        private readonly IOptions<MediaMessageConfiguration> _config;

        private readonly ILogger<MediaPacketDiscarder> _logger;

        public MediaPacketDiscarderTest()
        {
            _clientId = "123";

            _config = Options.Create(new MediaMessageConfiguration
            {
                MaxOutstandingMediaMessageSize = 100,
                MaxOutstandingMediaMessageCount = 10,
                TargetOutstandingMediaMessageSize = 50,
                TargetOutstandingMediaMessageCount = 5
            });

            _logger = Substitute.For<ILogger<MediaPacketDiscarder>>();
        }

        [Fact]
        public void ShouldDiscardMediaPacket_Should_ReturnFalse_When_NotDiscardable()
        {
            // Arrange
            var discarder = new MediaPacketDiscarder(_clientId, _config, _logger);

            // Act
            var result = discarder.ShouldDiscardMediaPacket(false, 200, 20);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void ShouldDiscardMediaPacket_Should_ReturnFalse_When_NotDiscarding()
        {
            // Arrange
            var discarder = new MediaPacketDiscarder(_clientId, _config, _logger);

            // Act
            var result = discarder.ShouldDiscardMediaPacket(true, 40, 4);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void ShouldDiscardMediaPacket_Should_ReturnFalse_When_WithinTargetRange()
        {
            // Arrange
            var discarder = new MediaPacketDiscarder(_clientId, _config, _logger);
            discarder.ShouldDiscardMediaPacket(true, 200, 20);

            // Act
            var result = discarder.ShouldDiscardMediaPacket(true, 40, 4);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void ShouldDiscardMediaPacket_Should_ReturnFalse_When_DiscardingAndAboveTargetRange()
        {
            // Arrange
            var discarder = new MediaPacketDiscarder(_clientId, _config, _logger);
            discarder.ShouldDiscardMediaPacket(true, 100, 100);

            // Act
            var result = discarder.ShouldDiscardMediaPacket(true, 70, 6);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public void ShouldDiscardMediaPacket_Should_ReturnTrue_When_AboveMaxThreshold()
        {
            // Arrange
            var discarder = new MediaPacketDiscarder(_clientId, _config, _logger);

            // Act
            var result = discarder.ShouldDiscardMediaPacket(true, 110, 11);

            // Assert
            result.Should().Be(true);
        }
    }
}
