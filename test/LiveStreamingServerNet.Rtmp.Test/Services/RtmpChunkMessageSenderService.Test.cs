using AutoFixture;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
{
    public class RtmpChunkMessageSenderServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IRtmpChunkMessageWriterService _writer;
        private readonly IRtmpChunkMessageSenderService _sut;

        public RtmpChunkMessageSenderServiceTest()
        {
            _fixture = new Fixture();
            _dataBufferPool = Substitute.For<IDataBufferPool>();
            _writer = Substitute.For<IRtmpChunkMessageWriterService>();
            _sut = new RtmpChunkMessageSenderService(_dataBufferPool, _writer);
        }

        [Fact]
        public void Send_Should_SendMessageChunksToClient()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientContext>();
            var basicHeader = _fixture.Create<RtmpChunkBasicHeader>();
            var messageHeader = _fixture.Create<RtmpChunkMessageHeaderType0>();
            var payloadWriter = _fixture.Create<Action<IDataBuffer>>();

            using IDataBuffer dataBuffer = new DataBuffer();
            using IDataBuffer payloadBuffer = new DataBuffer();

            var callback = Substitute.For<Action<bool>>();

            _dataBufferPool.Obtain().Returns(payloadBuffer);
            clientContext.Client.When(x => x.Send(Arg.Any<Action<IDataBuffer>>(), Arg.Any<Action<bool>>()))
                .Do(x =>
                {
                    x.Arg<Action<IDataBuffer>>().Invoke(dataBuffer);
                    x.Arg<Action<bool>>().Invoke(true);
                });

            // Act
            _sut.Send(clientContext, basicHeader, messageHeader, payloadWriter, callback);

            // Assert
            _dataBufferPool.Received(1).Obtain();
            _writer.Received(1).Write(dataBuffer, basicHeader, messageHeader, payloadBuffer, clientContext.OutChunkSize);
            clientContext.Client.Received(1).Send(Arg.Any<Action<IDataBuffer>>(), callback);
            callback.Received(1).Invoke(Arg.Any<bool>());
        }

        [Fact]
        public async Task SendAsync_Should_SendMessageChunksToClient()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientContext>();
            var basicHeader = _fixture.Create<RtmpChunkBasicHeader>();
            var messageHeader = _fixture.Create<RtmpChunkMessageHeaderType0>();
            var payloadWriter = _fixture.Create<Action<IDataBuffer>>();

            using IDataBuffer dataBuffer = new DataBuffer();
            using IDataBuffer payloadBuffer = new DataBuffer();

            _dataBufferPool.Obtain().Returns(payloadBuffer);
            clientContext.Client.When(x => x.Send(Arg.Any<Action<IDataBuffer>>(), Arg.Any<Action<bool>>()))
                .Do(x =>
                {
                    x.Arg<Action<IDataBuffer>>().Invoke(dataBuffer);
                    x.Arg<Action<bool>>().Invoke(true);
                });

            // Act
            await _sut.SendAsync(clientContext, basicHeader, messageHeader, payloadWriter);

            // Assert
            _dataBufferPool.Received(1).Obtain();
            _writer.Received(1).Write(dataBuffer, basicHeader, messageHeader, payloadBuffer, clientContext.OutChunkSize);
            clientContext.Client.Received(1).Send(Arg.Any<Action<IDataBuffer>>(), Arg.Any<Action<bool>>());
        }

        [Fact]
        public void Send_Should_SendMessageChunksToClients()
        {
            // Arrange
            var clientContext1 = Substitute.For<IRtmpClientContext>();
            var clientContext2 = Substitute.For<IRtmpClientContext>();

            var basicHeader = _fixture.Create<RtmpChunkBasicHeader>();
            var messageHeader = _fixture.Create<RtmpChunkMessageHeaderType0>();
            var payloadWriter = _fixture.Create<Action<IDataBuffer>>();

            // Act
            _sut.Send(new List<IRtmpClientContext> { clientContext1, clientContext2 }, basicHeader, messageHeader, payloadWriter);

            // Assert
            _dataBufferPool.Received().Obtain();
            _writer.Received().Write(Arg.Any<IDataBuffer>(), basicHeader, messageHeader, Arg.Any<IDataBuffer>(), Arg.Any<uint>());
            clientContext1.Client.Received(1).Send(Arg.Any<IDataBuffer>());
            clientContext2.Client.Received(1).Send(Arg.Any<IDataBuffer>());
        }
    }
}
