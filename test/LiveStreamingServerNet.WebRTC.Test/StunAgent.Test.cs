using FluentAssertions;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Stun.Configurations;
using LiveStreamingServerNet.WebRTC.Stun.Internal;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using LiveStreamingServerNet.WebRTC.Udp.Internal;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Test
{
    public class StunAgentTest
    {
        [Theory, Trait("Network", "External")]
        [InlineData("stun:stun.l.google.com:19302")]
        public async Task GetBindingResponse_Should_GetSuccessfulResponseFromStunServer(string stunServerUri)
        {
            // Arrange
            using var udpSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            ErrorBoundary.Execute(() => udpSocket.DontFragment = true);
            udpSocket.DualMode = true;
            udpSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));

            await using var udpTransport = new UdpTransport(udpSocket);

            var sender = new UdpStunSender(udpTransport);
            var config = new StunAgentConfiguration();

            await using var stunAgent = new StunAgent(sender, config);

            EventHandler<UdpPacketEventArgs> packetHandler = (_, args) =>
            {
                using var bufferReader = new RentedBufferReader(args.RentedBuffer);
                stunAgent.FeedPacket(bufferReader, args.RemoteEndPoint);
            };

            udpTransport.OnPacketReceived += packetHandler;

            try
            {
                // Act
                var started = udpTransport.Start();
                var resolver = new StunDnsResolver();

                var endpoints = await resolver.ResolveAsync(stunServerUri);
                var target = endpoints.FirstOrDefault();

                using var request = new StunMessage(StunClass.Request, StunConstants.BindingRequestMethod, new List<IStunAttribute>());

                // Assert
                started.Should().BeTrue();
                target.Should().NotBeNull();

                // Act
                var (response, _) = await stunAgent.SendRequestAsync(request, target!);

                // Assert
                response.Should().NotBeNull();
                response.Class.Should().Be(StunClass.SuccessResponse);
                response.Method.Should().Be(StunConstants.BindingRequestMethod);
                response.Attributes.Should().ContainItemsAssignableTo<XorMappedAddressAttribute>();
            }
            finally
            {
                udpTransport.OnPacketReceived -= packetHandler;
            }
        }
    }
}
