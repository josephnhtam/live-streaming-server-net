using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Test.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Commands.Dispatcher
{
    public class RtmpMessageDispatcherTest
    {
        private readonly IFixture _fixture;
        private readonly TestHandler _testHandler;
        private readonly Test2Handler _test2Handler;
        private readonly IServiceProvider _services;

        public RtmpMessageDispatcherTest()
        {
            _fixture = new Fixture();
            _testHandler = Substitute.For<TestHandler>();
            _test2Handler = Substitute.For<Test2Handler>();

            var map = new RtmpCommandHandlerMap(new Dictionary<string, Type> {
                { "test", typeof(TestHandler) },
                { "test2", typeof(Test2Handler) },
            });
            var logger = Substitute.For<ILogger<RtmpCommandDispatcher>>();

            var services = new ServiceCollection();
            services.AddSingleton(_testHandler)
                    .AddSingleton(_test2Handler)
                    .AddSingleton<IRtmpCommandDispatcher>(svc => new RtmpCommandDispatcher(svc, map, logger));
            _services = services.BuildServiceProvider();
        }

        [Theory]
        [InlineData(RtmpMessageType.CommandMessageAmf0)]
        [InlineData(RtmpMessageType.CommandMessageAmf3)]
        public async Task DispatchAsync_Should_DispatchTest1CommandToTest1Handler(byte messageTypeId)
        {
            // Arrange
            var sut = _services.GetRequiredService<IRtmpCommandDispatcher>();
            var clientContext = Substitute.For<IRtmpClientContext>();

            var commandName = "test";
            var transactionId = _fixture.Create<double>();
            var commandObject = new Dictionary<string, object> { { "key1", 1.0 }, { "key2", "value2" }, { "key3", true } };
            var publishingName = _fixture.Create<string>();

            var payloadBuffer = new NetBuffer();
            payloadBuffer.WriteAmf(new List<object?>
            {
               commandName, transactionId, new AmfArray(commandObject), publishingName
            }, messageTypeId == RtmpMessageType.CommandMessageAmf3 ? AmfEncodingType.Amf3 : AmfEncodingType.Amf0);
            payloadBuffer.MoveTo(0);

            var chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            chunkStreamContext.MessageHeader.MessageTypeId.Returns(messageTypeId);
            chunkStreamContext.MessageHeader.MessageLength.Returns(payloadBuffer.Size);

            var expectedResult = _fixture.Create<bool>();
            _testHandler.HandleAsync(chunkStreamContext, clientContext, Arg.Any<TestCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResult);

            // Act
            var result = await sut.DispatchAsync(chunkStreamContext, clientContext, payloadBuffer, default);

            // Assert
            await _testHandler.Received(1).HandleAsync(
                chunkStreamContext,
                clientContext,
                Arg.Is<TestCommand>(x =>
                    x.TransactionId == transactionId &&
                    x.CommandObject.Match(commandObject) &&
                    x.PublishingName == publishingName),
                Arg.Any<CancellationToken>());

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(RtmpMessageType.CommandMessageAmf0)]
        [InlineData(RtmpMessageType.CommandMessageAmf3)]
        public async Task DispatchAsync_Should_DispatchTest2CommandToTest2Handler(byte messageTypeId)
        {
            // Arrange
            var sut = _services.GetRequiredService<IRtmpCommandDispatcher>();
            var clientContext = Substitute.For<IRtmpClientContext>();

            var commandName = "test2";
            var transactionId = _fixture.Create<double>();
            var flag = _fixture.Create<bool>();
            var publishingName = _fixture.Create<string>();
            var commandObject = new Dictionary<string, object> { { "key1", 1.0 }, { "key2", "value2" }, { "key3", true } };

            var payloadBuffer = new NetBuffer();
            payloadBuffer.WriteAmf(new List<object?>
            {
               commandName, transactionId, flag, publishingName, new AmfArray(commandObject)
            }, messageTypeId == RtmpMessageType.CommandMessageAmf3 ? AmfEncodingType.Amf3 : AmfEncodingType.Amf0);
            payloadBuffer.MoveTo(0);

            var chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            chunkStreamContext.MessageHeader.MessageTypeId.Returns(messageTypeId);
            chunkStreamContext.MessageHeader.MessageLength.Returns(payloadBuffer.Size);

            var expectedResult = _fixture.Create<bool>();
            _test2Handler.HandleAsync(chunkStreamContext, clientContext, Arg.Any<Test2Command>(), Arg.Any<CancellationToken>())
                .Returns(expectedResult);

            // Act
            await sut.DispatchAsync(chunkStreamContext, clientContext, payloadBuffer, default);

            // Assert
            var result = await _test2Handler.Received(1).HandleAsync(
                  chunkStreamContext,
                  clientContext,
                  Arg.Is<Test2Command>(x =>
                      x.TransactionId == transactionId &&
                      x.Flag == flag &&
                      x.PublishingName == publishingName &&
                      x.CommandObject.Match(commandObject) &&
                      x.Optional == null),
                  Arg.Any<CancellationToken>());

            result.Should().Be(expectedResult);
        }

        internal record TestCommand(double TransactionId, IDictionary<string, object> CommandObject, string PublishingName);
        [RtmpCommand("test")] internal abstract class TestHandler : RtmpCommandHandler<TestCommand> { }

        internal record Test2Command(double TransactionId, bool Flag, string PublishingName, IDictionary<string, object> CommandObject, IDictionary<string, object>? Optional);
        [RtmpCommand("test2")] internal abstract class Test2Handler : RtmpCommandHandler<Test2Command> { }
    }
}
