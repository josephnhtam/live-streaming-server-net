using FluentAssertions;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Stun.Configurations;
using LiveStreamingServerNet.WebRTC.Stun.Internal;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Test
{
    public class StunPeerTest
    {
        [Theory, Trait("Network", "External")]
        [InlineData("stun:stun.l.google.com:19302")]
        public async Task GetBindingResponse_Should_GetSuccessfulResponseFromStunServer(string stunServerUri)
        {
            // Arrange
            const ushort bindingRequest = 0x0001;

            using var cts = new CancellationTokenSource();

            using var udpSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            ErrorBoundary.Execute(() => udpSocket.DontFragment = true);
            udpSocket.DualMode = true;
            udpSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));

            var sender = new SocketStunSender(udpSocket);
            var config = new StunPeerConfiguration();

            using var stunPeer = new StunPeer(sender, config);

            var receiveTask = Task.Run(async () =>
                await ReceiveUdpPacketAsync(udpSocket, stunPeer, cts.Token), cts.Token);

            try
            {
                // Act
                var resolver = new StunDnsResolver();

                var endpoints = await resolver.ResolveAsync(stunServerUri, cts.Token);
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

        private static async Task ReceiveUdpPacketAsync(Socket udpSocket, StunPeer stunPeer, CancellationToken cancellation)
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);

            while (!cancellation.IsCancellationRequested)
            {
                var buffer = DataBufferPool.Shared.Obtain();

                try
                {
                    buffer.Size = 2048;

                    var result = await udpSocket.ReceiveFromAsync(buffer.AsMemory(), remoteEndPoint, cancellation);
                    await stunPeer.FeedPacketAsync(buffer, (IPEndPoint)result.RemoteEndPoint, cancellation);
                }
                finally
                {
                    DataBufferPool.Shared.Recycle(buffer);
                }
            }
        }
    }
}
