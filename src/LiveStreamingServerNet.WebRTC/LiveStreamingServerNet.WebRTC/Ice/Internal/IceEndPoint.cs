using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
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

        public IceEndPoint(IUdpTransport udpTransport, IStunAgentFactory stunAgentFactory)
        {
            _udpTransport = udpTransport;
            _udpTransport.OnStateChanged += OnUdpStateChanged;
            _udpTransport.OnPacketReceived += OnUdpPacketReceived;

            _stunAgent = stunAgentFactory.Create(_udpTransport);
        }

        public IceEndPoint(Socket socket, IStunAgentFactory stunAgentFactory, IDataBufferPool? bufferPool = null)
            : this(new UdpTransport(socket, bufferPool), stunAgentFactory)
        {
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
                _stunAgent.FeedPacket(buffer, args.RemoteEndPoint);
                return;
            }

            OnPacketReceived?.Invoke(this, new IcePacketEventArgs(args.RentedBuffer, args.RemoteEndPoint));
        }

        public Task<(StunMessage, UnknownAttributes?)> SendStunRequestAsync(
            StunMessage request, IPEndPoint remoteEndPoint, CancellationToken cancellation = default)
        {
            return _stunAgent.SendRequestAsync(request, remoteEndPoint, cancellation);
        }

        public ValueTask SendStunIndicationAsync(
            StunMessage indication, IPEndPoint remoteEndPoint, CancellationToken cancellation = default)
        {
            return _stunAgent.SendIndicationAsync(indication, remoteEndPoint, cancellation);
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
