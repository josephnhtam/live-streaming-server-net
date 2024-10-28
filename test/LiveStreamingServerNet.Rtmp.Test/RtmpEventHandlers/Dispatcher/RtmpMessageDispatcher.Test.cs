using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Dispatcher
{
    public class RtmpMessageDispatcherTest
    {
        private readonly IFixture _fixture;
        private readonly TestHandler _testHandler;
        private readonly Test2Handler _test2Handler;
        private readonly ITestContext _context;
        private readonly DataBuffer _payloadBuffer;
        private readonly IRtmpMessageDispatcher<ITestContext> _sut;
        private readonly ILogger<RtmpMessageDispatcher<ITestContext>> _logger;
        private IRtmpChunkStreamContext _chunkStreamContext;

        public RtmpMessageDispatcherTest()
        {
            _fixture = new Fixture();
            _testHandler = Substitute.For<TestHandler>();
            _test2Handler = Substitute.For<Test2Handler>();

            _context = Substitute.For<ITestContext>();

            _payloadBuffer = new DataBuffer();
            _payloadBuffer.Write(_fixture.Create<byte[]>());
            _payloadBuffer.MoveTo(0);

            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _chunkStreamContext.PayloadBuffer.Returns(_payloadBuffer);

            _logger = Substitute.For<ILogger<RtmpMessageDispatcher<ITestContext>>>();

            var map = new RtmpMessageHandlerMap(new Dictionary<byte, Type> {
                { 1, typeof(TestHandler) },
                { 2, typeof(Test2Handler) },
            });

            var services = new ServiceCollection();
            services.AddSingleton(_testHandler)
                    .AddSingleton(_test2Handler)
                    .AddSingleton<IRtmpMessageDispatcher<ITestContext>>(svc =>
                        new RtmpMessageDispatcher<ITestContext>(svc, map, _logger));

            _sut = services.BuildServiceProvider().GetRequiredService<IRtmpMessageDispatcher<ITestContext>>();
        }

        [Fact]
        public async Task DispatchAsync_Should_DispatchMessageToTest1Handler()
        {
            // Arrange
            byte messageType = 1;
            _chunkStreamContext.MessageHeader.MessageTypeId.Returns(messageType);

            var expectedResult = _fixture.Create<bool>();
            _testHandler.HandleAsync(_chunkStreamContext, _context, _payloadBuffer, Arg.Any<CancellationToken>())
                .Returns(expectedResult);

            // Act
            var result = await _sut.DispatchAsync(_chunkStreamContext, _context, default);

            // Assert
            await _testHandler.Received(1).HandleAsync(
                _chunkStreamContext,
                _context,
                _payloadBuffer,
                Arg.Any<CancellationToken>());

            result.Should().Be(expectedResult);
        }

        [Fact]
        public async Task DispatchAsync_Should_DispatchMessageToTest2Handler()
        {
            // Arrange
            byte messageType = 2;

            _chunkStreamContext.MessageHeader.MessageTypeId.Returns(messageType);

            var expectedResult = _fixture.Create<bool>();
            _test2Handler.HandleAsync(_chunkStreamContext, _context, _payloadBuffer, Arg.Any<CancellationToken>())
                .Returns(expectedResult);

            // Act
            var result = await _sut.DispatchAsync(_chunkStreamContext, _context, default);

            // Assert
            await _test2Handler.Received(1).HandleAsync(
                _chunkStreamContext,
                _context,
                _payloadBuffer,
                Arg.Any<CancellationToken>());

            result.Should().Be(expectedResult);
        }

        [Fact]
        public async Task DispatchAsync_Should_ThrowInvalidOperationException_When_NoHandlerFound()
        {
            // Arrange
            byte messageType = 3;

            _chunkStreamContext.MessageHeader.MessageTypeId.Returns(messageType);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.DispatchAsync(_chunkStreamContext, _context, default));
        }

        internal interface ITestContext { }

        [RtmpMessageType(1)]
        internal abstract class TestHandler : IRtmpMessageHandler<ITestContext>
        {
            public abstract ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, ITestContext clientContext, IDataBuffer payloadBuffer, CancellationToken cancellationToken);
        }

        [RtmpMessageType(2)]
        internal abstract class Test2Handler : IRtmpMessageHandler<ITestContext>
        {
            public abstract ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, ITestContext clientContext, IDataBuffer payloadBuffer, CancellationToken cancellationToken);
        }
    }
}
