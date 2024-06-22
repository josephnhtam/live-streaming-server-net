using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Test.Utilities;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers
{
    public class RtmpChunkEventHandlerTest
    {
        [Theory]
        [MemberData(nameof(CreateTestParameters))]
        internal async Task Handle_Should_HandleChunksCorrectly<TRtmpChunkMessageHeader>(
            IRtmpClientContext clientContext,
            IRtmpChunkStreamContext streamContext,
            Stream stream,
            byte[] payload,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            // Arrange
            var dataBufferPool = new DataBufferPool(Options.Create(new DataBufferPoolConfiguration()));
            var dispatcher = Substitute.For<IRtmpMessageDispatcher>();
            var protocolControlMessageSender = Substitute.For<IRtmpProtocolControlMessageSenderService>();
            var logger = Substitute.For<ILogger<RtmpChunkEventHandler>>();

            using IDataBuffer resultPayloadBuffer = new DataBuffer();

            var tcs = new TaskCompletionSource();
            dispatcher.DispatchAsync(streamContext, clientContext, Arg.Any<CancellationToken>())
                .Returns(true)
                .AndDoes(x =>
                {
                    streamContext.PayloadBuffer!.ReadAndWriteTo(resultPayloadBuffer, streamContext.PayloadBuffer.Size);
                    resultPayloadBuffer.MoveTo(0);
                    tcs.SetResult();
                });

            var networkStream = new NetworkStream(stream);
            var sut = new RtmpChunkEventHandler(dataBufferPool, dispatcher, protocolControlMessageSender, logger);

            while (!tcs.Task.IsCompleted)
            {
                // Act
                var @event = new RtmpChunkEvent { ClientContext = clientContext, NetworkStream = networkStream };
                var result = await sut.Handle(@event, default);

                // Assert
                result.Succeeded.Should().Be(true);
            }

            // Assert
            if (messageHeader is RtmpChunkMessageHeaderType0 headerType0)
            {
                streamContext.ChunkStreamId.Should().Be(basicHeader.ChunkStreamId);
                streamContext.MessageHeader.Timestamp.Should().Be(headerType0.Timestamp);
                streamContext.MessageHeader.MessageLength.Should().Be(headerType0.MessageLength);
                streamContext.MessageHeader.MessageTypeId.Should().Be(headerType0.MessageTypeId);
                streamContext.MessageHeader.MessageStreamId.Should().Be(headerType0.MessageStreamId);
                streamContext.MessageHeader.HasExtendedTimestamp.Should().Be(headerType0.HasExtendedTimestamp());
            }
            else if (messageHeader is RtmpChunkMessageHeaderType1 headerType1)
            {
                streamContext.ChunkStreamId.Should().Be(basicHeader.ChunkStreamId);
                streamContext.MessageHeader.TimestampDelta.Should().Be(headerType1.TimestampDelta);
                streamContext.MessageHeader.MessageLength.Should().Be(headerType1.MessageLength);
                streamContext.MessageHeader.MessageTypeId.Should().Be(headerType1.MessageTypeId);
                streamContext.MessageHeader.HasExtendedTimestamp.Should().Be(headerType1.HasExtendedTimestamp());
            }
            else if (messageHeader is RtmpChunkMessageHeaderType2 headerType2)
            {
                streamContext.ChunkStreamId.Should().Be(basicHeader.ChunkStreamId);
                streamContext.MessageHeader.TimestampDelta.Should().Be(headerType2.TimestampDelta);
                streamContext.MessageHeader.HasExtendedTimestamp.Should().Be(headerType2.HasExtendedTimestamp());
            }

            _ = dispatcher.Received(1).DispatchAsync(streamContext, clientContext, Arg.Any<CancellationToken>());

            resultPayloadBuffer.UnderlyingBuffer.Take(resultPayloadBuffer.Size)
                .Should().BeEquivalentTo(payload);

            streamContext.PayloadBuffer.Should().BeNull();
        }

        public static IEnumerable<object[]> CreateTestParameters()
        {
            var fixture = new Fixture();

            {
                var chunkSize = 128u;
                var chunkStreamId = Helpers.CreateRandomChunkStreamId();

                var streamContext = new RtmpChunkStreamContext(chunkStreamId);
                var clientContext = Substitute.For<IRtmpClientContext>();
                clientContext.InChunkSize.Returns(chunkSize);
                clientContext.GetChunkStreamContext(chunkStreamId).Returns(streamContext);

                var payload = fixture.CreateMany<byte>(1000).ToArray();
                var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(
                    fixture.Create<uint>(), payload.Length, fixture.Create<byte>(), fixture.Create<uint>());

                yield return new object[]
                {
                    clientContext,
                    streamContext,
                    CreateStream(basicHeader, messageHeader, payload, chunkSize),
                    payload,
                    basicHeader,
                    messageHeader
                };
            }

            {
                var chunkSize = 2000u;
                var chunkStreamId = Helpers.CreateRandomChunkStreamId();

                var streamContext = new RtmpChunkStreamContext(chunkStreamId);
                var clientContext = Substitute.For<IRtmpClientContext>();
                clientContext.InChunkSize.Returns(chunkSize);
                clientContext.GetChunkStreamContext(chunkStreamId).Returns(streamContext);

                var payload = fixture.CreateMany<byte>(1000).ToArray();
                var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(
                    fixture.Create<uint>(), payload.Length, fixture.Create<byte>(), fixture.Create<uint>());

                yield return new object[]
                {
                    clientContext,
                    streamContext,
                    CreateStream(basicHeader, messageHeader, payload, chunkSize),
                    payload,
                    basicHeader,
                    messageHeader
                };
            }

            {
                var chunkSize = 500u;
                var chunkStreamId = Helpers.CreateRandomChunkStreamId();

                var streamContext = new RtmpChunkStreamContext(chunkStreamId);
                var clientContext = Substitute.For<IRtmpClientContext>();
                clientContext.InChunkSize.Returns(chunkSize);
                clientContext.GetChunkStreamContext(chunkStreamId).Returns(streamContext);

                var payload = fixture.CreateMany<byte>(1000).ToArray();
                var basicHeader = new RtmpChunkBasicHeader(1, chunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType1(
                    fixture.Create<uint>(), payload.Length, fixture.Create<byte>());

                yield return new object[]
                {
                    clientContext,
                    streamContext,
                    CreateStream(basicHeader, messageHeader, payload, chunkSize),
                    payload,
                    basicHeader,
                    messageHeader
                };
            }

            {
                var chunkSize = 500u;
                var chunkStreamId = Helpers.CreateRandomChunkStreamId();

                var streamContext = new RtmpChunkStreamContext(chunkStreamId);
                var clientContext = Substitute.For<IRtmpClientContext>();
                clientContext.InChunkSize.Returns(chunkSize);
                clientContext.GetChunkStreamContext(chunkStreamId).Returns(streamContext);

                var payload = new byte[0];
                var basicHeader = new RtmpChunkBasicHeader(2, chunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType2(fixture.Create<uint>());

                yield return new object[]
                {
                    clientContext,
                    streamContext,
                    CreateStream(basicHeader, messageHeader, payload, chunkSize),
                    payload,
                    basicHeader,
                    messageHeader
                };
            }
        }

        private static Stream CreateStream<TRtmpChunkMessageHeader>
            (RtmpChunkBasicHeader basicHeader, TRtmpChunkMessageHeader messageHeader, byte[] payload, uint chunkSize)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var writer = new RtmpChunkMessageWriterService();

            using var payloadBuffer = new DataBuffer();
            payloadBuffer.Write(payload);
            payloadBuffer.MoveTo(0);

            using var tempBuffer = new DataBuffer();
            writer.Write(tempBuffer, basicHeader, messageHeader, payloadBuffer, chunkSize);
            return new MemoryStream(tempBuffer.UnderlyingBuffer.Take(tempBuffer.Size).ToArray());
        }
    }
}
