using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LiveStreamingServerNet.Networking.Test
{
    public class ClientBufferSenderTest : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly ILogger<ClientBufferSender> _logger;
        private readonly IClientBufferSender _sut;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _cancellationToken;

        public ClientBufferSenderTest()
        {
            _fixture = new Fixture();
            _dataBufferPool = new DataBufferPool(Options.Create(new DataBufferPoolConfiguration()));
            _logger = Substitute.For<ILogger<ClientBufferSender>>();

            _sut = new ClientBufferSender(1, _dataBufferPool, _logger);

            _cts = new CancellationTokenSource();
            _cancellationToken = _cts.Token;
        }

        [Fact]
        public async Task SendAsync_Should_WriteBufferIntoNetworkStream()
        {
            // Arrange
            using var innerStream = new MemoryStream();
            using var networkStream = new NetworkStream(innerStream);
            var expectedBuffer = _fixture.Create<byte[]>();

            _sut.Start(networkStream, _cancellationToken);

            using var dataBuffer = new DataBuffer();
            dataBuffer.Write(expectedBuffer);

            // Act
            await _sut.SendAsync(dataBuffer);

            // Assert
            innerStream.Should().HaveLength(expectedBuffer.Length);

            innerStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await innerStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        [Fact]
        public async Task SendAsyncWithRentedBuffer_Should_WriteBufferIntoNetworkStream()
        {
            // Arrange
            using var innerStream = new MemoryStream();
            using var networkStream = new NetworkStream(innerStream);
            var expectedBuffer = _fixture.Create<byte[]>();

            _sut.Start(networkStream, _cancellationToken);

            var rentedBuffer = new RentedBuffer(expectedBuffer.Length);
            expectedBuffer.AsSpan().CopyTo(rentedBuffer.Buffer);

            // Act
            await _sut.SendAsync(rentedBuffer);

            rentedBuffer.Unclaim();

            // Assert
            innerStream.Should().HaveLength(expectedBuffer.Length);

            innerStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await innerStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        [Fact]
        public async Task SendAsyncWithWriter_Should_WriteBufferIntoNetworkStream()
        {
            // Arrange
            using var innerStream = new MemoryStream();
            using var networkStream = new NetworkStream(innerStream);
            var expectedBuffer = _fixture.Create<byte[]>();

            _sut.Start(networkStream, _cancellationToken);

            // Act
            await _sut.SendAsync(dataBuffer => dataBuffer.Write(expectedBuffer));

            // Assert
            innerStream.Should().HaveLength(expectedBuffer.Length);

            innerStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await innerStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        [Fact]
        public async Task Send_Should_WriteBufferIntoNetworkStream()
        {
            // Arrange
            using var innerStream = new MemoryStream();
            using var networkStream = new NetworkStream(innerStream);
            var expectedBuffer = _fixture.Create<byte[]>();

            _sut.Start(networkStream, _cancellationToken);

            using var dataBuffer = new DataBuffer();
            dataBuffer.Write(expectedBuffer);

            // Act
            var tcs = new TaskCompletionSource();
            _sut.Send(dataBuffer, _ => tcs.SetResult());
            await tcs.Task;

            // Assert
            innerStream.Should().HaveLength(expectedBuffer.Length);

            innerStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await innerStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        [Fact]
        public async Task SendWithRentedBuffer_Should_WriteBufferIntoNetworkStream()
        {
            // Arrange
            using var innerStream = new MemoryStream();
            using var networkStream = new NetworkStream(innerStream);
            var expectedBuffer = _fixture.Create<byte[]>();

            _sut.Start(networkStream, _cancellationToken);

            var rentedBuffer = new RentedBuffer(expectedBuffer.Length);
            expectedBuffer.AsSpan().CopyTo(rentedBuffer.Buffer);

            // Act
            var tcs = new TaskCompletionSource();
            _sut.Send(rentedBuffer, _ => tcs.SetResult());
            await tcs.Task;

            rentedBuffer.Unclaim();

            // Assert
            innerStream.Should().HaveLength(expectedBuffer.Length);

            innerStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await innerStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        [Fact]
        public async Task SendWithWriter_Should_WriteBufferIntoNetworkStream()
        {
            // Arrange
            using var innerStream = new MemoryStream();
            using var networkStream = new NetworkStream(innerStream);
            var expectedBuffer = _fixture.Create<byte[]>();

            _sut.Start(networkStream, _cancellationToken);

            // Act
            var tcs = new TaskCompletionSource();
            _sut.Send(dataBuffer => dataBuffer.Write(expectedBuffer), _ => tcs.SetResult());
            await tcs.Task;

            // Assert
            innerStream.Should().HaveLength(expectedBuffer.Length);

            innerStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await innerStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        [Fact]
        public async Task DataBufferSender_Should_BeCancellable()
        {
            // Arrange
            using var innerStream = new MemoryStream();
            using var networkStream = new NetworkStream(innerStream);

            _sut.Start(networkStream, _cancellationToken);

            // Act
            _cts.Cancel();
            var waitUntilComplete = _sut.DisposeAsync().AsTask();
            var result = await Task.WhenAny(waitUntilComplete, Task.Delay(10000));

            // Assert
            result.Should().Be(waitUntilComplete);
        }

        [Fact]
        public async Task Send_Should_WriteBufferIntoNetworkStreamInOrder()
        {
            // Arrange
            using var innerStream = new MemoryStream();
            using var networkStream = new NetworkStream(innerStream);
            var expectedBuffer = _fixture.CreateMany<byte>(100).ToArray();

            _sut.Start(networkStream, _cancellationToken);

            // Act
            var tcs = new TaskCompletionSource();

            for (int i = 0; i < expectedBuffer.Length; i++)
            {
                var idx = i;

                _sut.Send(dataBuffer => dataBuffer.Write(expectedBuffer[idx]), _ =>
                {
                    if (idx == expectedBuffer.Length - 1) tcs.SetResult();
                });
            }

            await tcs.Task;

            // Assert
            innerStream.Should().HaveLength(expectedBuffer.Length);

            innerStream.Position = 0;
            var actualBuffer = new byte[expectedBuffer.Length];
            await innerStream.ReadAsync(actualBuffer, 0, expectedBuffer.Length);

            actualBuffer.Should().Equal(expectedBuffer);
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
