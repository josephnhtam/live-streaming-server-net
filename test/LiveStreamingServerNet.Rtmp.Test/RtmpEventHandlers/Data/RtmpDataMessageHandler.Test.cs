using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Data;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Test.Utilities;
using LiveStreamingServerNet.Utilities.Buffers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Data
{
    public class RtmpDataMessageHandlerTest
    {
        private readonly IRtmpClientContext _clientContext;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpPublishStreamContext _publishStreamContext;
        private readonly DataBuffer _payloadBuffer;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly ILogger<RtmpDataMessageHandler> _logger;

        private readonly RtmpDataMessageHandler _sut;

        public RtmpDataMessageHandlerTest()
        {
            _clientContext = Substitute.For<IRtmpClientContext>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            _payloadBuffer = new DataBuffer();
            _mediaMessageCacher = Substitute.For<IRtmpMediaMessageCacherService>();
            _eventDispatcher = Substitute.For<IRtmpServerStreamEventDispatcher>();
            _logger = Substitute.For<ILogger<RtmpDataMessageHandler>>();
            _sut = new RtmpDataMessageHandler(_mediaMessageCacher, _eventDispatcher, _logger);

            var amfEncodingType = AmfEncodingType.Amf0;
            var metaData = new Dictionary<string, object>() { { "framerate", 60.0 } };
            var metaDataCommandValues = new List<object>
            {
                RtmpDataMessageConstants.SetDataFrame,
                RtmpDataMessageConstants.OnMetaData,
                metaData.ToAmfArray()
            };

            _payloadBuffer.WriteAmf(metaDataCommandValues, amfEncodingType);
            _payloadBuffer.MoveTo(0);

            _chunkStreamContext.MessageHeader.MessageTypeId.Returns(
                amfEncodingType == AmfEncodingType.Amf0 ? RtmpMessageType.DataMessageAmf0 : RtmpMessageType.DataMessageAmf3);
        }

        [Fact]
        public async Task HandleAsync_Should_CacheAndBroadcastMetaDatas_For_MetaDataMessage_When_StreamExists()
        {
            // Arrange
            _clientContext.PublishStreamContext.Returns(_publishStreamContext);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, _payloadBuffer, default);

            // Assert
            _publishStreamContext.Received(1).StreamMetaData = Helpers.CreateExpectedMetaData("framerate", 60.0);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnFalse_For_MetaDataMessage_When_StreamDoesntExist()
        {
            // Arrange
            _clientContext.PublishStreamContext.Returns((IRtmpPublishStreamContext?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, _payloadBuffer, default);

            // Assert
            result.Should().BeFalse();
        }
    }
}
