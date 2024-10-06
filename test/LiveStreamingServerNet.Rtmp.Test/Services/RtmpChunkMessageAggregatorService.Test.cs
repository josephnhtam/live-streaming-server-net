using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Test.Utilities;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
{
    public class RtmpChunkMessageAggregatorServiceTest
    {
        [Theory]
        [MemberData(nameof(CreateTestParameters))]
        internal async Task AggregateChunkMessagesAsync_Should_AggregateChunksCorrectly<TRtmpChunkMessageHeader>(
            IRtmpChunkStreamContextProvider chunkStreamContextProvider,
            IRtmpChunkStreamContext chunkStreamContext,
            MemoryStream stream,
            byte[] payload,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            // Arrange
            var initialTimestamp = chunkStreamContext.Timestamp;
            var dataBufferPool = new DataBufferPool(Options.Create(new DataBufferPoolConfiguration()));
            var networkStream = new NetworkStream(stream);
            var sut = new RtmpChunkMessageAggregatorService(dataBufferPool);

            long totalSize = 0;

            while (true)
            {
                // Act
                var result = await sut.AggregateChunkMessagesAsync(networkStream, chunkStreamContextProvider, default);
                totalSize += result.ChunkMessageSize;

                if (result.IsComplete)
                {
                    break;
                }
            }

            // Assert
            totalSize.Should().Be(stream.Length);

            if (messageHeader is RtmpChunkMessageHeaderType0 headerType0)
            {
                chunkStreamContext.ChunkStreamId.Should().Be(basicHeader.ChunkStreamId);
                chunkStreamContext.MessageHeader.MessageLength.Should().Be(headerType0.MessageLength);
                chunkStreamContext.MessageHeader.MessageTypeId.Should().Be(headerType0.MessageTypeId);
                chunkStreamContext.MessageHeader.MessageStreamId.Should().Be(headerType0.MessageStreamId);
                chunkStreamContext.MessageHeader.HasExtendedTimestamp.Should().Be(headerType0.HasExtendedTimestamp());
                chunkStreamContext.Timestamp.Should().Be(headerType0.Timestamp);
            }
            else if (messageHeader is RtmpChunkMessageHeaderType1 headerType1)
            {
                chunkStreamContext.ChunkStreamId.Should().Be(basicHeader.ChunkStreamId);
                chunkStreamContext.MessageHeader.MessageLength.Should().Be(headerType1.MessageLength);
                chunkStreamContext.MessageHeader.MessageTypeId.Should().Be(headerType1.MessageTypeId);
                chunkStreamContext.MessageHeader.HasExtendedTimestamp.Should().Be(headerType1.HasExtendedTimestamp());
                chunkStreamContext.Timestamp.Should().Be(initialTimestamp + chunkStreamContext.MessageHeader.Timestamp);
            }
            else if (messageHeader is RtmpChunkMessageHeaderType2 headerType2)
            {
                chunkStreamContext.ChunkStreamId.Should().Be(basicHeader.ChunkStreamId);
                chunkStreamContext.MessageHeader.HasExtendedTimestamp.Should().Be(headerType2.HasExtendedTimestamp());
                chunkStreamContext.Timestamp.Should().Be(initialTimestamp + chunkStreamContext.MessageHeader.Timestamp);
            }

            chunkStreamContext.PayloadBuffer!.AsSpan(0, chunkStreamContext.PayloadBuffer!.Size).ToArray()
                .Should().BeEquivalentTo(payload);
        }

        public static IEnumerable<object[]> CreateTestParameters()
        {
            var fixture = new Fixture();

            {
                var chunkSize = 128u;
                var chunkStreamId = Helpers.CreateRandomChunkStreamId();

                var chunkStreamContext = new RtmpChunkStreamContext(chunkStreamId);
                var chunkStreamContextProvider = Substitute.For<IRtmpChunkStreamContextProvider>();
                chunkStreamContext.Timestamp = fixture.Create<uint>();
                chunkStreamContextProvider.InChunkSize.Returns(chunkSize);
                chunkStreamContextProvider.GetChunkStreamContext(chunkStreamId).Returns(chunkStreamContext);

                var payload = fixture.CreateMany<byte>(1000).ToArray();
                var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(
                    fixture.Create<uint>(), payload.Length, fixture.Create<byte>(), fixture.Create<uint>());

                yield return new object[]
                {
                    chunkStreamContextProvider,
                    chunkStreamContext,
                    CreateStream(basicHeader, messageHeader, payload, chunkSize),
                    payload,
                    basicHeader,
                    messageHeader
                };
            }

            {
                var chunkSize = 2000u;
                var chunkStreamId = Helpers.CreateRandomChunkStreamId();

                var chunkStreamContext = new RtmpChunkStreamContext(chunkStreamId);
                var chunkStreamContextProvider = Substitute.For<IRtmpChunkStreamContextProvider>();
                chunkStreamContext.Timestamp = fixture.Create<uint>();
                chunkStreamContextProvider.InChunkSize.Returns(chunkSize);
                chunkStreamContextProvider.GetChunkStreamContext(chunkStreamId).Returns(chunkStreamContext);

                var payload = fixture.CreateMany<byte>(1000).ToArray();
                var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(
                    fixture.Create<uint>(), payload.Length, fixture.Create<byte>(), fixture.Create<uint>());

                yield return new object[]
                {
                    chunkStreamContextProvider,
                    chunkStreamContext,
                    CreateStream(basicHeader, messageHeader, payload, chunkSize),
                    payload,
                    basicHeader,
                    messageHeader
                };
            }

            {
                var chunkSize = 500u;
                var chunkStreamId = Helpers.CreateRandomChunkStreamId();

                var chunkStreamContext = new RtmpChunkStreamContext(chunkStreamId);
                var chunkStreamContextProvider = Substitute.For<IRtmpChunkStreamContextProvider>();
                chunkStreamContext.Timestamp = fixture.Create<uint>();
                chunkStreamContextProvider.InChunkSize.Returns(chunkSize);
                chunkStreamContextProvider.GetChunkStreamContext(chunkStreamId).Returns(chunkStreamContext);

                var payload = fixture.CreateMany<byte>(1000).ToArray();
                var basicHeader = new RtmpChunkBasicHeader(1, chunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType1(
                    fixture.Create<uint>(), payload.Length, fixture.Create<byte>());

                yield return new object[]
                {
                    chunkStreamContextProvider,
                    chunkStreamContext,
                    CreateStream(basicHeader, messageHeader, payload, chunkSize),
                    payload,
                    basicHeader,
                    messageHeader
                };
            }

            {
                var chunkSize = 500u;
                var chunkStreamId = Helpers.CreateRandomChunkStreamId();

                var chunkStreamContext = new RtmpChunkStreamContext(chunkStreamId);
                var chunkStreamContextProvider = Substitute.For<IRtmpChunkStreamContextProvider>();
                chunkStreamContext.Timestamp = fixture.Create<uint>();
                chunkStreamContextProvider.InChunkSize.Returns(chunkSize);
                chunkStreamContextProvider.GetChunkStreamContext(chunkStreamId).Returns(chunkStreamContext);

                var payload = new byte[0];
                var basicHeader = new RtmpChunkBasicHeader(2, chunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType2(fixture.Create<uint>());

                yield return new object[]
                {
                    chunkStreamContextProvider,
                    chunkStreamContext,
                    CreateStream(basicHeader, messageHeader, payload, chunkSize),
                    payload,
                    basicHeader,
                    messageHeader
                };
            }
        }

        private static MemoryStream CreateStream<TRtmpChunkMessageHeader>
            (RtmpChunkBasicHeader basicHeader, TRtmpChunkMessageHeader messageHeader, byte[] payload, uint chunkSize)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var writer = new RtmpChunkMessageWriterService();

            using var payloadBuffer = new DataBuffer();
            payloadBuffer.Write(payload);
            payloadBuffer.MoveTo(0);

            using var tempBuffer = new DataBuffer();
            writer.Write(tempBuffer, basicHeader, messageHeader, payloadBuffer, chunkSize);
            return new MemoryStream(tempBuffer.AsSpan().ToArray());
        }
    }
}
