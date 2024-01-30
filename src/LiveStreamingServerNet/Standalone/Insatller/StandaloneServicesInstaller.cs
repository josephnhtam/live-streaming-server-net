using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using LiveStreamingServerNet.Standalone.Internal;
using LiveStreamingServerNet.Standalone.Internal.Services;
using LiveStreamingServerNet.Standalone.Internal.Services.Contracts;
using LiveStreamingServerNet.Standalone.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Standalone.Insatller
{
    public static class StandaloneServicesInstaller
    {
        public static IRtmpServerConfigurator AddStandaloneServices(this IRtmpServerConfigurator configurator)
        {
            var services = configurator.Services;

            services.AddSingleton<IRtmpStreamManagerService, RtmpStreamManagerService>()
                    .AddSingleton<IRtmpStreamManagerApiService, RtmpStreamManagerApiService>();

            configurator.AddStreamEventHandler<RtmpServerStreamEventListener>();

            return configurator;
        }
    }
}
