using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Installer
{
    internal class RtmpClientConfigurator : IRtmpClientConfigurator
    {
        public IServiceCollection Services { get; }

        public RtmpClientConfigurator(IServiceCollection services)
        {
            Services = services;
        }
    }
}