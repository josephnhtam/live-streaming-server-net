using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Flv.Internal;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test.Services
{
    public class FlvMediaTagSenderServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IFlvClient _flvClient;
        private readonly IFlvMediaTagSenderService _sut;

        public FlvMediaTagSenderServiceTest()
        {
            _fixture = new Fixture();
            _flvClient = Substitute.For<IFlvClient>();
            _sut = new FlvMediaTagSenderService();
        }

        [Theory]
        [InlineData(MediaType.Audio)]
        [InlineData(MediaType.Video)]
        public async Task SendMediaTagAsync_Should_SendMediaTag(MediaType mediaType)
        {
            // Arrange
            var payloadBuffer = _fixture.Create<byte[]>();
            var payloadSize = payloadBuffer.Length;
            var timestamp = _fixture.Create<uint>();

            var expectedFlvType = mediaType == MediaType.Video ? FlvTagType.Video : FlvTagType.Audio;

            using var netBuffer = new NetBuffer();

            _flvClient.When(x => x.WriteTagAsync(expectedFlvType, timestamp, Arg.Any<Action<INetBuffer>>(), Arg.Any<CancellationToken>()))
                .Do(x => x.Arg<Action<INetBuffer>>().Invoke(netBuffer));

            // Act
            await _sut.SendMediaTagAsync(_flvClient, mediaType, payloadBuffer, payloadSize, timestamp, default);

            // Assert
            await _flvClient.Received(1).WriteTagAsync(expectedFlvType, timestamp, Arg.Any<Action<INetBuffer>>(), Arg.Any<CancellationToken>());
            netBuffer.UnderlyingBuffer.Take(netBuffer.Size).ToArray().Should().BeEquivalentTo(payloadBuffer.Take(payloadSize).ToArray());
        }
    }
}
