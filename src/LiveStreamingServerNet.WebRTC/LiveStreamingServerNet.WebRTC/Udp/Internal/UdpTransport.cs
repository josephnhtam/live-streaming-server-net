using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Udp.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Utilities;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace LiveStreamingServerNet.WebRTC.Udp.Internal
{
    public class UdpTransport : IUdpTransport
    {
        public IPEndPoint LocalEndPoint { get; }
        public UdpTransportState State => GetState();

        public event EventHandler<UdpTransportState>? OnStateChanged;
        public event EventHandler<UdpPacketEventArgs>? OnPacketReceived;

        private readonly Socket _socket;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly int _maxDatagramSize;
        private readonly Channel<UdpPacket> _sendChannel;
        private readonly Channel<UdpPacket> _receiveChannel;
        private readonly CancellationTokenSource _cts;
        private readonly object _stateLock = new();

        private Task? _sendLoopTask;
        private Task? _receiveLoopTask;
        private Task? _dispatchLoopTask;

        private UdpTransportState _state = UdpTransportState.New;
        private volatile int _isDisposed;

        public UdpTransport(Socket socket, IDataBufferPool? dataBufferPool = null, int maxDatagramSize = 2048)
        {
            LocalEndPoint = (socket.LocalEndPoint as IPEndPoint)!;

            _socket = socket;
            _dataBufferPool = dataBufferPool ?? DataBufferPool.Shared;
            _maxDatagramSize = maxDatagramSize;

            _sendChannel = Channel.CreateUnbounded<UdpPacket>(new()
            {
                AllowSynchronousContinuations = true,
                SingleReader = true
            });

            _receiveChannel = Channel.CreateUnbounded<UdpPacket>(new()
            {
                AllowSynchronousContinuations = true,
                SingleWriter = true,
                SingleReader = true
            });

            _cts = new CancellationTokenSource();
        }

        public bool SendPacket(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEndPoint)
        {
            if (State != UdpTransportState.Started)
                return false;

            var dataBuffer = _dataBufferPool.Obtain();

            try
            {
                dataBuffer.Write(buffer.Span);

                var packet = new UdpPacket(dataBuffer, remoteEndPoint);
                if (!_sendChannel.Writer.TryWrite(packet))
                {
                    _dataBufferPool.Recycle(dataBuffer);
                    return false;
                }

                return true;
            }
            catch
            {
                _dataBufferPool.Recycle(dataBuffer);
                return false;
            }
        }

        public bool Start()
        {
            if (!TryTransitionTo(UdpTransportState.Started, UdpTransportState.New))
                return false;

            var token = _cts.Token;

            _sendLoopTask = Task.Run(() => ProcessSendLoopAsync(token), token);
            _receiveLoopTask = Task.Run(() => ProcessReceiveLoopAsync(token), token);
            _dispatchLoopTask = Task.Run(() => ProcessDispatchLoopAsync(token), token);

            return true;
        }

        public bool Close()
        {
            if (!TryTransitionTo(UdpTransportState.Closed, UdpTransportState.New, UdpTransportState.Started))
                return false;

            _sendChannel.Writer.TryComplete();
            _receiveChannel.Writer.TryComplete();
            _cts.Cancel();

            ErrorBoundary.Execute(() => _socket.Close());
            return true;
        }

        private async Task ProcessSendLoopAsync(CancellationToken cancellation)
        {
            try
            {
                await foreach (var packet in _sendChannel.Reader.ReadAllAsync(cancellation))
                {
                    var (buffer, remoteEndPoint) = packet;

                    try
                    {
                        await _socket.SendToAsync(
                            buffer.AsMemory(0, buffer.Position),
                            SocketFlags.None,
                            remoteEndPoint,
                            cancellation);
                    }
                    catch (SocketException ex) when (!ex.SocketErrorCode.IsFatal()) { }
                    catch (ObjectDisposedException) { }
                    finally
                    {
                        _dataBufferPool.Recycle(buffer);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ChannelClosedException) { }
            finally
            {
                Close();
            }
        }

        private async Task ProcessReceiveLoopAsync(CancellationToken cancellation)
        {
            var remoteEndPoint = _socket.AddressFamily == AddressFamily.InterNetworkV6
                ? new IPEndPoint(IPAddress.IPv6Any, 0)
                : new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var buffer = _dataBufferPool.Obtain();
                    buffer.Size = _maxDatagramSize;

                    try
                    {
                        var result = await _socket.ReceiveFromAsync(
                            buffer.AsMemory(0, _maxDatagramSize),
                            SocketFlags.None,
                            remoteEndPoint,
                            cancellation);

                        var receivedBytes = result.ReceivedBytes;
                        if (receivedBytes == 0)
                        {
                            _dataBufferPool.Recycle(buffer);
                            break;
                        }

                        buffer.Size = receivedBytes;

                        var packet = new UdpPacket(buffer, (IPEndPoint)result.RemoteEndPoint);
                        if (!_receiveChannel.Writer.TryWrite(packet))
                            throw new ChannelClosedException();
                    }
                    catch (SocketException ex) when (!ex.SocketErrorCode.IsFatal())
                    {
                        _dataBufferPool.Recycle(buffer);
                    }
                    catch (ObjectDisposedException)
                    {
                        _dataBufferPool.Recycle(buffer);
                        break;
                    }
                    catch
                    {
                        _dataBufferPool.Recycle(buffer);
                        throw;
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ChannelClosedException) { }
            finally
            {
                Close();
            }
        }

        private async Task ProcessDispatchLoopAsync(CancellationToken cancellation)
        {
            try
            {
                await foreach (var packet in _receiveChannel.Reader.ReadAllAsync(cancellation))
                {
                    var (buffer, remoteEndPoint) = packet;
                    var rentedBuffer = buffer.ToRentedBuffer();

                    try
                    {
                        OnPacketReceived?.Invoke(this, new UdpPacketEventArgs(rentedBuffer, remoteEndPoint));
                    }
                    catch { }
                    finally
                    {
                        rentedBuffer.Unclaim();
                        _dataBufferPool.Recycle(buffer);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ChannelClosedException) { }
            finally
            {
                Close();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
                return;

            Close();

            var tasks = new[] { _sendLoopTask, _receiveLoopTask, _dispatchLoopTask }
                .Where(t => t != null)
                .Select(t => t!);

            await ErrorBoundary.ExecuteAsync(async () => await Task.WhenAll(tasks));

            while (_sendChannel.Reader.TryRead(out var packet))
                _dataBufferPool.Recycle(packet.Buffer);

            while (_receiveChannel.Reader.TryRead(out var packet))
                _dataBufferPool.Recycle(packet.Buffer);

            ErrorBoundary.Execute(() => _socket.Dispose());

            _cts.Dispose();
        }

        private UdpTransportState GetState()
        {
            lock (_stateLock)
            {
                return _state;
            }
        }

        private bool TryTransitionTo(UdpTransportState newState, params UdpTransportState[] expected)
        {
            lock (_stateLock)
            {
                var current = _state;

                if (expected is { Length: > 0 } && Array.IndexOf(expected, current) < 0)
                    return false;

                if (current == newState)
                    return false;

                _state = newState;
            }

            ErrorBoundary.Execute(() => OnStateChanged?.Invoke(this, newState));
            return true;
        }

        private record struct UdpPacket(IDataBuffer Buffer, IPEndPoint RemoteEndPoint);
    }
}
