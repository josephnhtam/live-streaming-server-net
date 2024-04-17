using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;

namespace LiveStreamingServerNet.Rtmp.Test
{
    public class StreamUtilitiesTest
    {
        [Fact]
        public void ParseStreamName_Should_Return_StreamName()
        {
            // Arrange
            var rawStreamName = "streamName/streamKey";

            // Act
            var (streamName, queryStringMap) = StreamUtilities.ParseStreamName(rawStreamName);

            // Assert
            streamName.Should().Be("streamName/streamKey");

            queryStringMap.Should().NotBeNull();
            queryStringMap.Should().HaveCount(0);
        }

        [Fact]
        public void ParseStreamName_Should_Return_StreamName_And_QueryStringMap()
        {
            // Arrange
            var rawStreamName = "streamName/streamKey?param1=value1&param2=value2";

            // Act
            var (streamName, queryStringMap) = StreamUtilities.ParseStreamName(rawStreamName);

            // Assert
            streamName.Should().Be("streamName/streamKey");

            queryStringMap.Should().NotBeNull();
            queryStringMap.Should().HaveCount(2);
            queryStringMap.Should().ContainKey("param1").And.ContainValue("value1");
            queryStringMap.Should().ContainKey("param2").And.ContainValue("value2");
        }

        [Theory]
        [InlineData("stream", "app/demo", "/stream/app/demo")]
        [InlineData("stream", "", "/stream")]
        [InlineData("", "app/demo", "/app/demo")]
        public void ComposeStreamPath_Should_Return_CorrectStreamPath(string appName, string streamName, string expectedStreamPath)
        {
            // Act
            var streamPath = StreamUtilities.ComposeStreamPath(appName, streamName);

            // Assert
            streamPath.Should().Be(expectedStreamPath);
        }
    }
}
