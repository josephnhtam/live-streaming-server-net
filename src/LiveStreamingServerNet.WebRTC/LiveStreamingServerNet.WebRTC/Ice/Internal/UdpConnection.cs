using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Utilities;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    public class UdpConnection : IUdpConnection
    {
        public IPEndPoint LocalEndPoint { get; }

        public event EventHandler<UdpPacketEventArgs?>? OnPacketReceived;

        private int _state = (int)UdpConnectionState.Created;
        public UdpConnectionState State => (UdpConnectionState)Volatile.Read(ref _state);

        private readonly Socket _socket;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly Channel<UdpPacket> _sendChannel;
        private readonly Channel<UdpPacket> _receiveChannel;
        private readonly CancellationTokenSource _cts;

        private Task? _sendLoopTask;
        private Task? _receiveLoopTask;
        private Task? _dispatchLoopTask;
        private int _isClosedNotified;

        private const int MaxDatagramSize = 2048;

        public UdpConnection(Socket socket, IDataBufferPool? dataBufferPool)
        {
            LocalEndPoint = (socket.LocalEndPoint as IPEndPoint)!;

            _socket = socket;
            _dataBufferPool = dataBufferPool ?? DataBufferPool.Shared;

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
            var state = (UdpConnectionState)Volatile.Read(ref _state);
            if (state is UdpConnectionState.Closed or UdpConnectionState.Disposed)
            {
                return false;
            }

            if (Volatile.Read(ref _isClosedNotified) == 1)
            {
                return false;
            }

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
            catch (Exception)
            {
                _dataBufferPool.Recycle(dataBuffer);
                return false;
            }
        }

        public bool Start()
        {
            var prev = (UdpConnectionState)Interlocked.CompareExchange(
                ref _state, (int)UdpConnectionState.Started, (int)UdpConnectionState.Created);

            if (prev != UdpConnectionState.Created)
            {
                return false;
            }

            var token = _cts.Token;

            _sendLoopTask = Task.Run(() => ProcessSendLoopAsync(token), token);
            _receiveLoopTask = Task.Run(() => ProcessReceiveLoopAsync(token), token);
            _dispatchLoopTask = Task.Run(() => ProcessDispatchLoopAsync(token), token);
            return true;
        }

        private void NotifyClosed()
        {
            if (Interlocked.Exchange(ref _isClosedNotified, 1) != 0)
            {
                return;
            }

            Interlocked.CompareExchange(ref _state, (int)UdpConnectionState.Closed, (int)UdpConnectionState.Created);
            Interlocked.CompareExchange(ref _state, (int)UdpConnectionState.Closed, (int)UdpConnectionState.Started);

            ErrorBoundary.Execute(() => OnPacketReceived?.Invoke(this, null));
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
                            buffer.AsMemory(0, buffer.Position), SocketFlags.None, remoteEndPoint, cancellation);
                    }
                    catch (SocketException ex) when (!ex.SocketErrorCode.IsFatal()) { }
                    finally
                    {
                        _dataBufferPool.Recycle(buffer);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ChannelClosedException) { }
            catch (Exception)
            {
                _socket.Close();
            }
            finally
            {
                NotifyClosed();
            }
        }

        private async Task ProcessReceiveLoopAsync(CancellationToken cancellation)
        {
            EndPoint remoteEndPoint = _socket.AddressFamily == AddressFamily.InterNetworkV6
                ? new IPEndPoint(IPAddress.IPv6Any, 0)
                : new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var buffer = _dataBufferPool.Obtain();
                    buffer.Size = MaxDatagramSize;

                    try
                    {
                        var result = await _socket.ReceiveFromAsync(
                            buffer.AsMemory(0, MaxDatagramSize), SocketFlags.None, remoteEndPoint, cancellation);

                        var receivedBytes = result.ReceivedBytes;
                        if (receivedBytes == 0)
                        {
                            _dataBufferPool.Recycle(buffer);
                            break;
                        }

                        buffer.Size = receivedBytes;

                        var packet = new UdpPacket(buffer, (IPEndPoint)result.RemoteEndPoint);
                        if (!_receiveChannel.Writer.TryWrite(packet))
                        {
                            throw new ChannelClosedException();
                        }
                    }
                    catch (SocketException ex) when (!ex.SocketErrorCode.IsFatal())
                    {
                        _dataBufferPool.Recycle(buffer);
                    }
                    catch (Exception)
                    {
                        _dataBufferPool.Recycle(buffer);
                        throw;
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ChannelClosedException) { }
            catch (Exception)
            {
                _socket.Close();
            }
            finally
            {
                NotifyClosed();
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
                    catch (Exception) { }
                    finally
                    {
                        rentedBuffer.Unclaim();
                        _dataBufferPool.Recycle(buffer);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ChannelClosedException) { }
            catch (Exception)
            {
                _socket.Close();
            }
            finally
            {
                NotifyClosed();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _state, (int)UdpConnectionState.Disposed) == (int)UdpConnectionState.Disposed)
            {
                return;
            }

            _sendChannel.Writer.TryComplete();
            _receiveChannel.Writer.TryComplete();

            _cts.Cancel();

            List<Task?> tasks = [_sendLoopTask, _receiveLoopTask, _dispatchLoopTask];
            await ErrorBoundary.ExecuteAsync(async () =>
                await Task.WhenAll(tasks.Where(t => t != null)!)
            );

            while (_sendChannel.Reader.TryRead(out var packet))
            {
                _dataBufferPool.Recycle(packet.Buffer);
            }

            while (_receiveChannel.Reader.TryRead(out var packet))
            {
                _dataBufferPool.Recycle(packet.Buffer);
            }

            ErrorBoundary.Execute(() =>
            {
                _socket.Close();
                _socket.Dispose();
            });

            NotifyClosed();

            _cts.Dispose();
        }

        private record struct UdpPacket(IDataBuffer Buffer, IPEndPoint RemoteEndPoint);
    }
}
