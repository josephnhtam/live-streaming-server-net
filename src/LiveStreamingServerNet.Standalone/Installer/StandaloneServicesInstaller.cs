using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Services;
using LiveStreamingServerNet.Standalone.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Standalone.Installer
{
    public static class StandaloneServicesInstaller
    {
        public static IRtmpServerConfigurator AddStandaloneServices(this IRtmpServerConfigurator configurator)
        {
            var services = configurator.Services;

            services.AddSingleton<IRtmpStreamManagerApiService, RtmpStreamManagerApiService>();

            return configurator;
        }
    }
}
