using FluentAssertions;
using LiveStreamingServerNet.Utilities.PacketDiscarders;

namespace LIveStreamingServerNet.Utilities.Test
{
    public class PacketDiscarderTest
    {
        private readonly PacketDiscarderConfiguration _config;

        public PacketDiscarderTest()
        {
            _config = new PacketDiscarderConfiguration
            {
                MaxOutstandingPacketsSize = 100,
                MaxOutstandingPacketsCount = 10,
                TargetOutstandingPacketsSize = 50,
                TargetOutstandingPacketsCount = 5
            };
        }

        [Fact]
        public void ShouldDiscardPacket_Should_ReturnFalse_When_NotDiscardable()
        {
            // Arrange
            var discarder = new PacketDiscarder(_config);

            // Act
            var result = discarder.ShouldDiscardPacket(false, 200, 20);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void ShouldDiscardPacket_Should_ReturnFalse_When_NotDiscarding()
        {
            // Arrange
            var discarder = new PacketDiscarder(_config);

            // Act
            var result = discarder.ShouldDiscardPacket(true, 40, 4);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void ShouldDiscardPacket_Should_ReturnFalse_When_WithinTargetRange()
        {
            // Arrange
            var discarder = new PacketDiscarder(_config);
            discarder.ShouldDiscardPacket(true, 200, 20);

            // Act
            var result = discarder.ShouldDiscardPacket(true, 40, 4);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void ShouldDiscardPacket_Should_ReturnFalse_When_DiscardingAndAboveTargetRange()
        {
            // Arrange
            var discarder = new PacketDiscarder(_config);
            discarder.ShouldDiscardPacket(true, 100, 100);

            // Act
            var result = discarder.ShouldDiscardPacket(true, 70, 6);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public void ShouldDiscardPacket_Should_ReturnTrue_When_AboveMaxThreshold()
        {
            // Arrange
            var discarder = new PacketDiscarder(_config);

            // Act
            var result = discarder.ShouldDiscardPacket(true, 110, 11);

            // Assert
            result.Should().Be(true);
        }
    }
}
