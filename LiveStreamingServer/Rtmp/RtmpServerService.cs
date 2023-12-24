using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core;
using LiveStreamingServer.Rtmp.Core.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServer.Rtmp
{
    //public class RtmpServer : Server
    //{
    //    private readonly IRtmpServerContext _serverContext;
    //    private readonly IRtmpHandshakeHandler _handshakeHandler;

    //    public RtmpServer(ILogger? logger = null) : base(new NetBufferPool(), logger)
    //    {
    //        _serverContext = new RtmpServerContext();
    //        _handshakeHandler = CreateHandshakeHandler();
    //    }

    //    protected sealed override IClientPeerHandler CreateClientPeerHandler(IClientPeer clientPeer)
    //    {
    //        var peerContext = new RtmpClientPeerContext();
    //        return new RtmpClientPeerHandler(this, clientPeer, _serverContext, peerContext, _handshakeHandler);
    //    }

    //    protected virtual IRtmpHandshakeHandler CreateHandshakeHandler()
    //    {
    //        return new RtmpHandshakeHandler();
    //    }
    //}

    //public class RtmpServer : IServer
    //{
    //    public RtmpServer()
    //    {
    //        var builder =Host.CreateEmptyApplicationBuilder();
    //    }

    //    public bool IsStarted => throw new NotImplementedException();

    //    public IList<IClientPeerHandle> ClientPeers => throw new NotImplementedException();

    //    public IClientPeerHandle? GetClientPeer(uint clientPeerId)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Task RunAsync(IPEndPoint localEndpoint, CancellationToken cancellationToken = default)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
