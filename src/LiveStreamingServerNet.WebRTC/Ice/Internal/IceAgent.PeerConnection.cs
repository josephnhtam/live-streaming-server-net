using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Exceptions;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal partial class IceAgent
    {
        private event EventHandler<IceCandidatePairPacketEventArgs>? _onPairPacketReceived;

        private void OnCandidatePairCreated(IceCandidatePair pair)
        {
            pair.LocalCandidate.IceEndPoint.OnPacketReceived += (_, args) =>
            {
                OnPacketReceived(pair, args);
            };
        }

        private bool SendPacket(IceCandidatePair pair, ReadOnlyMemory<byte> packet)
        {
            return pair.LocalCandidate.IceEndPoint.SendPacket(packet, pair.RemoteCandidate.EndPoint);
        }

        private void OnPacketReceived(IceCandidatePair pair, IcePacketEventArgs args)
        {
            _onPairPacketReceived?.Invoke(this, new IceCandidatePairPacketEventArgs(pair, args));
        }

        public async ValueTask<IIcePeerConnection> AcceptAsync(CancellationToken cancellation = default)
        {
            var tcs = new TaskCompletionSource<IIcePeerConnection>();

            void StateChanged(IceConnectionState state)
            {
                try
                {
                    var peerConnection = CreatePeerConnection(state);

                    if (peerConnection != null)
                        tcs.TrySetResult(peerConnection);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            IIcePeerConnection? CreatePeerConnection(IceConnectionState state)
            {
                if (state == IceConnectionState.Connected)
                    return new PeerConnection(this);

                if (state == IceConnectionState.Failed)
                    throw new IceConnectionFailedException();

                if (state == IceConnectionState.Closed)
                    throw new IceConnectionClosedException();

                return null;
            }

            await using var registration = cancellation.Register(() => tcs.TrySetCanceled(cancellation));

            OnStateChanged += StateChanged;

            try
            {
                lock (_syncLock)
                {
                    var peerConnection = CreatePeerConnection(ConnectionState);

                    if (peerConnection != null)
                        return peerConnection;
                }

                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                OnStateChanged -= StateChanged;
            }
        }

        private record struct IceCandidatePairPacketEventArgs(IceCandidatePair Pair, IcePacketEventArgs PacketArgs);

        private class PeerConnection : IIcePeerConnection
        {
            private readonly IceAgent _agent;
            private volatile int _disposed;

            public event EventHandler<IcePacketEventArgs>? OnPacketReceived;

            public PeerConnection(IceAgent agent)
            {
                _agent = agent;
                agent._onPairPacketReceived += OnPairPacketReceived;
            }

            private void OnPairPacketReceived(object? sender, IceCandidatePairPacketEventArgs args)
            {
                if (_disposed == 1)
                    return;

                try
                {
                    OnPacketReceived?.Invoke(this, args.PacketArgs);
                }
                catch (Exception ex)
                {
                    // todo: add logs
                }
            }

            public bool SendPacket(ReadOnlyMemory<byte> buffer)
            {
                if (_disposed == 1)
                    return false;

                var selectedPair = _agent._selectedPair;

                if (selectedPair == null)
                    return false;

                return _agent.SendPacket(selectedPair, buffer);
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 1)
                    return;

                _agent._onPairPacketReceived -= OnPairPacketReceived;
            }
        }
    }
}
