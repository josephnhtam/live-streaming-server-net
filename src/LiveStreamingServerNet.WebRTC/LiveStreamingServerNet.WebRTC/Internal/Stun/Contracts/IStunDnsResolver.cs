using System.Net;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Contracts
{
    internal interface IStunDnsResolver
    {
        Task<IPEndPoint[]> ResolveAsync(string stunUri, CancellationToken cancellationToken = default);
    }
}
