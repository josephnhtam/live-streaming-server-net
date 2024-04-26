using AutoFixture;
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
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Commands.Dispatcher
{
    public class RtmpMessageDispatcherTest
    {
        private readonly IFixture _fixture;
        private readonly TestHandler _testHandler;
        private readonly IServiceProvider _services;

        public RtmpMessageDispatcherTest()
        {
            _fixture = new Fixture();
            _testHandler = Substitute.For<TestHandler>();

            var map = new RtmpCommandHandlerMap(new Dictionary<string, Type> { { "test", typeof(TestHandler) } });
            var logger = Substitute.For<ILogger<RtmpCommandDispatcher>>();

            var services = new ServiceCollection();
            services.AddSingleton(_testHandler);
            services.AddSingleton<IRtmpCommandDispatcher>(svc => new RtmpCommandDispatcher(svc, map, logger));
            _services = services.BuildServiceProvider();
        }

        [Theory]
        [InlineData(RtmpMessageType.CommandMessageAmf0)]
        [InlineData(RtmpMessageType.CommandMessageAmf3)]
        public async Task DispatchAsync(byte messageTypeId)
        {
            // Arrange
            var sut = _services.GetRequiredService<IRtmpCommandDispatcher>();
            var clientContext = Substitute.For<IRtmpClientContext>();

            var commandName = "test";
            var transactionId = _fixture.Create<double>();
            var commandObject = new Dictionary<string, object> { { "key1", 1.0 }, { "key2", "value2" } };
            var publishingName = _fixture.Create<string>();

            var payloadBuffer = new NetBuffer();
            payloadBuffer.WriteAmf(new List<object?>
            {
               commandName, transactionId, new AmfArray(commandObject), publishingName
            }, messageTypeId == RtmpMessageType.CommandMessageAmf3 ? AmfEncodingType.Amf3 : AmfEncodingType.Amf0);

            var chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            chunkStreamContext.MessageHeader.MessageTypeId.Returns(messageTypeId);
            chunkStreamContext.MessageHeader.MessageLength.Returns(payloadBuffer.Position);

            payloadBuffer.MoveTo(0);

            // Act
            await sut.DispatchAsync(chunkStreamContext, clientContext, payloadBuffer, default);

            // Assert
            await _testHandler.Received(1).HandleAsync(
                chunkStreamContext,
                clientContext,
                Arg.Is<TestCommand>(x =>
                    x.TransactionId == transactionId &&
                    x.CommandObject.Match(commandObject) &&
                    x.PublishingName == publishingName),
                Arg.Any<CancellationToken>());
        }

        internal record TestCommand(double TransactionId, IDictionary<string, object> CommandObject, string PublishingName);

        [RtmpCommand("test")]
        internal abstract class TestHandler : RtmpCommandHandler<TestCommand> { }
    }
}
