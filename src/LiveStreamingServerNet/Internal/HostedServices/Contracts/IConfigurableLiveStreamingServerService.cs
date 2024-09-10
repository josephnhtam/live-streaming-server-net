using LiveStreamingServerNet.Networking;
using Microsoft.Extensions.Hosting;

namespace LiveStreamingServerNet.Internal.HostedServices.Contracts
{
    internal interface IConfigurableLiveStreamingServerService : IHostedService
    {
        void ConfigureEndPoints(IReadOnlyList<ServerEndPoint> serverEndPoints);
    }
}
