using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;

namespace LiveStreamingServerNet.Rtmp.Test
{
    public class StreamUtilitiesTest
    {
        [Fact]
        public void ParseStreamPath_Should_Return_StreamName()
        {
            // Arrange
            var streamPath = "streamName/streamKey";

            // Act
            var (streamName, queryStringMap) = StreamUtilities.ParseStreamPath(streamPath);

            // Assert
            streamName.Should().Be("streamName/streamKey");

            queryStringMap.Should().NotBeNull();
            queryStringMap.Should().HaveCount(0);
        }

        [Fact]
        public void ParseStreamPath_Should_Return_StreamName_And_QueryStringMap()
        {
            // Arrange
            var streamPath = "streamName/streamKey?param1=value1&param2=value2";

            // Act
            var (streamName, queryStringMap) = StreamUtilities.ParseStreamPath(streamPath);

            // Assert
            streamName.Should().Be("streamName/streamKey");

            queryStringMap.Should().NotBeNull();
            queryStringMap.Should().HaveCount(2);
            queryStringMap.Should().ContainKey("param1").And.ContainValue("value1");
            queryStringMap.Should().ContainKey("param2").And.ContainValue("value2");
        }
    }
}
