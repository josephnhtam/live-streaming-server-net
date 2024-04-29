using AutoFixture;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
{
    public class RtmpChunkMessageSenderServiceTest
    {
        private readonly IFixture _fixture;
        private readonly INetBufferPool _netBufferPool;
        private readonly IRtmpChunkMessageWriterService _writer;
        private readonly IRtmpChunkMessageSenderService _sut;

        public RtmpChunkMessageSenderServiceTest()
        {
            _fixture = new Fixture();
            _netBufferPool = Substitute.For<INetBufferPool>();
            _writer = Substitute.For<IRtmpChunkMessageWriterService>();
            _sut = new RtmpChunkMessageSenderService(_netBufferPool, _writer);
        }

        [Fact]
        public void Send_Should_SendMessageChunksToClient()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientContext>();
            var basicHeader = _fixture.Create<RtmpChunkBasicHeader>();
            var messageHeader = _fixture.Create<RtmpChunkMessageHeaderType0>();
            var payloadWriter = _fixture.Create<Action<INetBuffer>>();

            using INetBuffer netBuffer = new NetBuffer();
            using INetBuffer payloadBuffer = new NetBuffer();

            var callback = Substitute.For<Action<bool>>();

            _netBufferPool.Obtain().Returns(payloadBuffer);
            clientContext.Client.When(x => x.Send(Arg.Any<Action<INetBuffer>>(), Arg.Any<Action<bool>>()))
                .Do(x =>
                {
                    x.Arg<Action<INetBuffer>>().Invoke(netBuffer);
                    x.Arg<Action<bool>>().Invoke(true);
                });

            // Act
            _sut.Send(clientContext, basicHeader, messageHeader, payloadWriter, callback);

            // Assert
            _netBufferPool.Received(1).Obtain();
            _writer.Received(1).Write(netBuffer, basicHeader, messageHeader, payloadBuffer, clientContext.OutChunkSize);
            clientContext.Client.Received(1).Send(Arg.Any<Action<INetBuffer>>(), callback);
            callback.Received(1).Invoke(Arg.Any<bool>());
        }

        [Fact]
        public async Task SendAsync_Should_SendMessageChunksToClient()
        {
            // Arrange
            var clientContext = Substitute.For<IRtmpClientContext>();
            var basicHeader = _fixture.Create<RtmpChunkBasicHeader>();
            var messageHeader = _fixture.Create<RtmpChunkMessageHeaderType0>();
            var payloadWriter = _fixture.Create<Action<INetBuffer>>();

            using INetBuffer netBuffer = new NetBuffer();
            using INetBuffer payloadBuffer = new NetBuffer();

            _netBufferPool.Obtain().Returns(payloadBuffer);
            clientContext.Client.When(x => x.Send(Arg.Any<Action<INetBuffer>>(), Arg.Any<Action<bool>>()))
                .Do(x =>
                {
                    x.Arg<Action<INetBuffer>>().Invoke(netBuffer);
                    x.Arg<Action<bool>>().Invoke(true);
                });

            // Act
            await _sut.SendAsync(clientContext, basicHeader, messageHeader, payloadWriter);

            // Assert
            _netBufferPool.Received(1).Obtain();
            _writer.Received(1).Write(netBuffer, basicHeader, messageHeader, payloadBuffer, clientContext.OutChunkSize);
            clientContext.Client.Received(1).Send(Arg.Any<Action<INetBuffer>>(), Arg.Any<Action<bool>>());
        }

        [Fact]
        public void Send_Should_SendMessageChunksToClients()
        {
            // Arrange
            var clientContext1 = Substitute.For<IRtmpClientContext>();
            var clientContext2 = Substitute.For<IRtmpClientContext>();

            var basicHeader = _fixture.Create<RtmpChunkBasicHeader>();
            var messageHeader = _fixture.Create<RtmpChunkMessageHeaderType0>();
            var payloadWriter = _fixture.Create<Action<INetBuffer>>();

            // Act
            _sut.Send(new List<IRtmpClientContext> { clientContext1, clientContext2 }, basicHeader, messageHeader, payloadWriter);

            // Assert
            _netBufferPool.Received().Obtain();
            _writer.Received().Write(Arg.Any<INetBuffer>(), basicHeader, messageHeader, Arg.Any<INetBuffer>(), Arg.Any<uint>());
            clientContext1.Client.Received(1).Send(Arg.Any<INetBuffer>());
            clientContext2.Client.Received(1).Send(Arg.Any<INetBuffer>());
        }
    }
}
