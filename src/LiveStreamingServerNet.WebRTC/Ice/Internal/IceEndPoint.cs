using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Udp.Internal;
using LiveStreamingServerNet.WebRTC.Udp.Internal.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal class IceEndPoint : IIceEndPoint
    {
        private readonly IUdpTransport _udpTransport;
        private readonly IStunAgent _stunAgent;
        private int _isDisposed;

        public event EventHandler<UdpTransportState>? OnStateChanged;
        public event EventHandler<IcePacketEventArgs>? OnPacketReceived;

        public UdpTransportState State => _udpTransport.State;

        public IceEndPoint(Socket socket, IUdpTransportFactory transportFactory, IStunAgentFactory stunAgentFactory)
        {
            _udpTransport = transportFactory.Create(socket);
            _udpTransport.OnStateChanged += OnUdpStateChanged;
            _udpTransport.OnPacketReceived += OnUdpPacketReceived;

            _stunAgent = stunAgentFactory.Create(_udpTransport);
        }

        public bool Start()
        {
            return _udpTransport.Start();
        }

        public bool Close()
        {
            return _udpTransport.Close();
        }

        private void OnUdpStateChanged(object? sender, UdpTransportState state)
        {
            OnStateChanged?.Invoke(this, state);
        }

        private void OnUdpPacketReceived(object? sender, UdpPacketEventArgs args)
        {
            using var buffer = new RentedBufferReader(args.RentedBuffer);

            if (StunMessage.ValidateHeader(buffer))
            {
                _stunAgent.FeedPacket(buffer, args.RemoteEndPoint, state: this);
                return;
            }

            OnPacketReceived?.Invoke(this, new IcePacketEventArgs(args.RentedBuffer, args.RemoteEndPoint));
        }

        public Task<StunResponse> SendStunRequestAsync(
            StunMessage request, IPEndPoint remoteEndPoint, CancellationToken cancellation = default)
        {
            return _stunAgent.SendRequestAsync(request, remoteEndPoint, cancellation);
        }

        public ValueTask SendStunIndicationAsync(
            StunMessage indication, IPEndPoint remoteEndPoint, CancellationToken cancellation = default)
        {
            return _stunAgent.SendIndicationAsync(indication, remoteEndPoint, cancellation);
        }

        public bool SendPacket(ReadOnlyMemory<byte> packet, IPEndPoint remoteEndPoint)
        {
            return _udpTransport.SendPacket(packet, remoteEndPoint);
        }

        public void SetStunMessageHandler(IStunMessageHandler? handler)
        {
            _stunAgent.SetMessageHandler(handler);
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            {
                return;
            }

            await _stunAgent.DisposeAsync().ConfigureAwait(false);
            await _udpTransport.DisposeAsync().ConfigureAwait(false);
        }
    }
}
