using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Test.Utilities;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
{
    public class RtmpCommandMessageSenderServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly RtmpCommandMessageSenderService _commandMessageSender;

        public RtmpCommandMessageSenderServiceTest()
        {
            _fixture = new Fixture();
            _chunkMessageSender = Substitute.For<IRtmpChunkMessageSenderService>();
            _commandMessageSender = new RtmpCommandMessageSenderService(_chunkMessageSender);
        }

        [Fact]
        public void SendCommandMessage_Should_SendCommandMessage()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientContext>();
            var chunkStreamId = Helpers.CreateRandomChunkStreamId();
            var commandName = _fixture.Create<string>();
            var transactionId = _fixture.Create<double>();
            var commandObject = _fixture.Create<Dictionary<string, object>>();
            var parameters = _fixture.Create<List<object?>>();
            var amfEncodingType = _fixture.Create<AmfEncodingType>();
            var callback = _fixture.Create<Action<bool>>();

            using var expectedBuffer = new DataBuffer();
            var expectedParameter = GetParameters(commandName, transactionId, commandObject, parameters);
            expectedBuffer.WriteAmf(expectedParameter, amfEncodingType);

            using var dataBuffer = new DataBuffer();

            _chunkMessageSender.When(x => x.Send(
                clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == chunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageTypeId ==
                        (amfEncodingType == AmfEncodingType.Amf0 ?
                        RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3) &&
                    x.MessageStreamId == 0
                ),
                Arg.Any<Action<IDataBuffer>>(),
                callback
            )).Do(x =>
            {
                x.Arg<Action<IDataBuffer>>().Invoke(dataBuffer);
                callback.Invoke(true);
            });

            // Act
            _commandMessageSender.SendCommandMessage(
                clientContext,
                chunkStreamId,
                commandName,
                transactionId,
                commandObject,
                parameters,
                amfEncodingType,
                callback);

            // Assert
            _chunkMessageSender.Received(1).Send(
                clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == chunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageTypeId ==
                        (amfEncodingType == AmfEncodingType.Amf0 ?
                        RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3) &&
                    x.MessageStreamId == 0
                ),
                Arg.Any<Action<IDataBuffer>>(),
                callback);

            dataBuffer.UnderlyingBuffer.Take(dataBuffer.Size).Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));
        }

        [Fact]
        public async Task SendCommandMessageAsync_Should_SendCommandMessage()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientContext>();
            var chunkStreamId = Helpers.CreateRandomChunkStreamId();
            var commandName = _fixture.Create<string>();
            var transactionId = _fixture.Create<double>();
            var commandObject = _fixture.Create<Dictionary<string, object>>();
            var parameters = _fixture.Create<List<object?>>();
            var amfEncodingType = _fixture.Create<AmfEncodingType>();

            using var expectedBuffer = new DataBuffer();
            var expectedParameter = GetParameters(commandName, transactionId, commandObject, parameters);
            expectedBuffer.WriteAmf(expectedParameter, amfEncodingType);

            using var dataBuffer = new DataBuffer();

            _chunkMessageSender.When(x => x.Send(
                clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == chunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageTypeId ==
                        (amfEncodingType == AmfEncodingType.Amf0 ?
                        RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3) &&
                    x.MessageStreamId == 0
                ),
                Arg.Any<Action<IDataBuffer>>(),
                Arg.Any<Action<bool>>()
            )).Do(x =>
            {
                x.Arg<Action<IDataBuffer>>().Invoke(dataBuffer);
                x.Arg<Action<bool>>().Invoke(true);
            });

            // Act
            await _commandMessageSender.SendCommandMessageAsync(
                clientContext,
                chunkStreamId,
                commandName,
                transactionId,
                commandObject,
                parameters,
                amfEncodingType
            );

            // Assert
            _chunkMessageSender.Received(1).Send(
                clientContext,
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == chunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageTypeId ==
                        (amfEncodingType == AmfEncodingType.Amf0 ?
                        RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3) &&
                    x.MessageStreamId == 0
                ),
                Arg.Any<Action<IDataBuffer>>(),
                Arg.Any<Action<bool>>());

            dataBuffer.UnderlyingBuffer.Take(dataBuffer.Size).Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));
        }

        [Fact]
        public void SendCommandMessageAsync_Should_BroadcastCommandMessages()
        {
            // Arrange
            var clientContexts = new List<IRtmpClientContext> { Substitute.For<IRtmpClientContext>(), Substitute.For<IRtmpClientContext>() };
            var chunkStreamId = Helpers.CreateRandomChunkStreamId();
            var commandName = _fixture.Create<string>();
            var transactionId = _fixture.Create<double>();
            var commandObject = _fixture.Create<Dictionary<string, object>>();
            var parameters = _fixture.Create<List<object?>>();
            var amfEncodingType = _fixture.Create<AmfEncodingType>();

            using var expectedBuffer = new DataBuffer();
            var expectedParameter = GetParameters(commandName, transactionId, commandObject, parameters);
            expectedBuffer.WriteAmf(expectedParameter, amfEncodingType);

            using var dataBuffer = new DataBuffer();

            _chunkMessageSender.When(x => x.Send(
                Arg.Is<IReadOnlyList<IRtmpClientContext>>(x => x.SequenceEqual(clientContexts)),
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == chunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageTypeId ==
                        (amfEncodingType == AmfEncodingType.Amf0 ?
                        RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3) &&
                    x.MessageStreamId == 0
                ),
                Arg.Any<Action<IDataBuffer>>()
            )).Do(x =>
            {
                x.Arg<Action<IDataBuffer>>().Invoke(dataBuffer);
            });

            // Act
            _commandMessageSender.SendCommandMessage(
                clientContexts,
                chunkStreamId,
                commandName,
                transactionId,
                commandObject,
                parameters,
                amfEncodingType
            );

            // Assert
            _chunkMessageSender.Received(1).Send(
                Arg.Is<IReadOnlyList<IRtmpClientContext>>(x => x.SequenceEqual(clientContexts)),
                Arg.Is<RtmpChunkBasicHeader>(x =>
                    x.ChunkType == 0 &&
                    x.ChunkStreamId == chunkStreamId
                ),
                Arg.Is<RtmpChunkMessageHeaderType0>(x =>
                    x.Timestamp == 0 &&
                    x.MessageTypeId ==
                        (amfEncodingType == AmfEncodingType.Amf0 ?
                        RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3) &&
                    x.MessageStreamId == 0
                ),
                Arg.Any<Action<IDataBuffer>>());

            dataBuffer.UnderlyingBuffer.Take(dataBuffer.Size).Should().BeEquivalentTo(expectedBuffer.UnderlyingBuffer.Take(expectedBuffer.Size));
        }

        private static List<object?> GetParameters(string commandName, double transactionId,
            IReadOnlyDictionary<string, object>? commandObject, IReadOnlyList<object?> additionalParameters)
        {
            var parameters = new List<object?>
            {
                commandName,
                transactionId,
                commandObject
            };

            return parameters.Concat(additionalParameters).ToList();
        }
    }
}
