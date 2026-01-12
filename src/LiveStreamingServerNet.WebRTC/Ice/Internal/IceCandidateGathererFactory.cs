using LiveStreamingServerNet.WebRTC.Ice.Configurations;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal class IceCandidateGathererFactory : IIceCandidateGathererFactory
    {
        private readonly IUdpTransportFactory _transportFactory;
        private readonly IStunAgentFactory _stunAgentFactory;
        private readonly IStunDnsResolver _stunDnsResolver;
        private readonly IceGathererConfiguration _config;
        private readonly ILoggerFactory _loggerFactory;

        public IceCandidateGathererFactory(
            IUdpTransportFactory transportFactory,
            IStunAgentFactory stunAgentFactory,
            IStunDnsResolver stunDnsResolver,
            IceGathererConfiguration config,
            ILoggerFactory loggerFactory)
        {
            _transportFactory = transportFactory;
            _stunAgentFactory = stunAgentFactory;
            _stunDnsResolver = stunDnsResolver;
            _config = config;
            _loggerFactory = loggerFactory;
        }

        public IIceCandidateGatherer Create(string identifier, IceCandidateTypeFlag candidateTypes = IceCandidateTypeFlag.All)
        {
            return new IceCandidateGatherer(
                identifier,
                candidateTypes,
                _transportFactory,
                _stunAgentFactory,
                _stunDnsResolver,
                _config,
                _loggerFactory.CreateLogger<IceCandidateGatherer>()
            );
        }
    }
}
