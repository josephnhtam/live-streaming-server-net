using System.Net;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts
{
    internal interface IStunDnsResolver
    {
        Task<IPEndPoint[]> ResolveAsync(string stunUri, CancellationToken cancellationToken = default);
    }
}
