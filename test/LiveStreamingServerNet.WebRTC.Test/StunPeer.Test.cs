using FluentAssertions;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Internal.Stun;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Configurations;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Test
{
    public class StunPeerTest
    {
        [Fact]
        public async Task GetBindingResponse_Should_GetSuccessfulResponseFromGoogleStunServer()
        {
            // Arrange
            const ushort bindingRequest = 0x0001;

            using var cts = new CancellationTokenSource();

            using var udpSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            ErrorBoundary.Execute(() => udpSocket.DontFragment = true);
            udpSocket.DualMode = true;
            udpSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));

            var sender = new SocketStunSender(udpSocket);
            var config = new StunClientConfiguration
            {
                RetransmissionTimeout = 500,
                MaxRetransmissions = 3
            };

            using var stunPeer = new StunPeer(sender, config);

            var receiveTask = Task.Run(async () =>
            {
                var remoteEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);

                while (!cts.Token.IsCancellationRequested)
                {
                    using var buffer = DataBufferPool.Shared.Obtain();
                    buffer.Size = 2048;

                    var result = await udpSocket.ReceiveFromAsync(buffer.AsMemory(), remoteEndPoint, cts.Token);
                    await stunPeer.FeedPacketAsync(buffer, (IPEndPoint)result.RemoteEndPoint, cts.Token);
                }
            }, cts.Token);

            try
            {
                // Act
                var resolver = new StunDnsResolver();
                var googleStunUri = "stun:stun.l.google.com:19302";

                var endpoints = await resolver.ResolveAsync(googleStunUri, cts.Token);
                var target = endpoints.FirstOrDefault();

                // Assert
                target.Should().NotBeNull();

                // Act
                var (response, _) = await stunPeer.SendRequestAsync(bindingRequest, new List<IStunAttribute>(), target!, cts.Token);

                // Assert
                response.Should().NotBeNull();
                response.Class.Should().Be(StunClass.SuccessResponse);
                response.Method.Should().Be(bindingRequest);
                response.Attributes.Should().ContainItemsAssignableTo<XorMappedAddressAttribute>();
            }
            finally
            {
                cts.Cancel();

                try
                {
                    await receiveTask;
                }
                catch { }
            }
        }
    }
}
