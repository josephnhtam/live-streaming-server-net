using FluentAssertions;
using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Networking.Test
{
    public class NetBufferSenderTest : IDisposable
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly ILogger<NetBufferSender> _logger;
        private readonly INetBufferSender _sut;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _cancellationToken;

        public NetBufferSenderTest()
        {
            _netBufferPool = new NetBufferPool(Options.Create(new NetBufferPoolConfiguration()));
            _logger = Substitute.For<ILogger<NetBufferSender>>();

            _sut = new NetBufferSender(1, _netBufferPool, _logger);

            _cts = new CancellationTokenSource();
            _cancellationToken = _cts.Token;
        }

        [Fact]
        public async Task SendAsync_Should_WriteBufferIntoNetworkStream()
        {
            // Arrange
            using var networkStream = new MemoryStream();
            var expectedBuffer = new byte[] { 1, 2, 3, 4, 5 };

            _sut.Start(networkStream, _cancellationToken);

            using var netBuffer = _netBufferPool.Obtain();
            netBuffer.Write(expectedBuffer);

            // Act
            await _sut.SendAsync(netBuffer);

            // Assert
            networkStream.Should().HaveLength(expectedBuffer.Length);

            networkStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await networkStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        [Fact]
        public async Task SendAsyncWithWriter_Should_WriteBufferIntoNetworkStream()
        {
            // Arrange
            using var networkStream = new MemoryStream();
            var expectedBuffer = new byte[] { 1, 2, 3, 4, 5 };

            _sut.Start(networkStream, _cancellationToken);

            // Act
            await _sut.SendAsync(netBuffer => netBuffer.Write(expectedBuffer));

            // Assert
            networkStream.Should().HaveLength(expectedBuffer.Length);

            networkStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await networkStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        [Fact]
        public async Task Send_Should_WriteBufferIntoNetworkStream()
        {
            // Arrange
            using var networkStream = new MemoryStream();
            var expectedBuffer = new byte[] { 1, 2, 3, 4, 5 };

            _sut.Start(networkStream, _cancellationToken);

            using var netBuffer = _netBufferPool.Obtain();
            netBuffer.Write(expectedBuffer);

            // Act
            var tcs = new TaskCompletionSource();
            _sut.Send(netBuffer, _ => tcs.SetResult());
            await tcs.Task;

            // Assert
            networkStream.Should().HaveLength(expectedBuffer.Length);

            networkStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await networkStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        [Fact]
        public async Task SendWithWriter_Should_WriteBufferIntoNetworkStream()
        {
            // Arrange
            using var networkStream = new MemoryStream();
            var expectedBuffer = new byte[] { 1, 2, 3, 4, 5 };

            _sut.Start(networkStream, _cancellationToken);

            // Act
            var tcs = new TaskCompletionSource();
            _sut.Send(netBuffer => netBuffer.Write(expectedBuffer), _ => tcs.SetResult());
            await tcs.Task;

            // Assert
            networkStream.Should().HaveLength(expectedBuffer.Length);

            networkStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await networkStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        [Fact]
        public async Task NetBufferSender_Should_BeCancellable()
        {
            // Arrange
            using var networkStream = new MemoryStream();

            _sut.Start(networkStream, _cancellationToken);

            // Act
            _cts.Cancel();
            var waitUntilComplete = _sut.DisposeAsync().AsTask();
            var result = await Task.WhenAny(waitUntilComplete, Task.Delay(10000));

            // Assert
            result.Should().Be(waitUntilComplete);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _netBufferPool.Dispose();
        }
    }
}
