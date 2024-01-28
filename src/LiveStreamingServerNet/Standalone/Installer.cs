using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using LiveStreamingServerNet.Standalone.Internal;
using LiveStreamingServerNet.Standalone.Internal.Services;
using LiveStreamingServerNet.Standalone.Internal.Services.Contracts;
using LiveStreamingServerNet.Standalone.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Standalone
{
    public static class Installer
    {
        public static IRtmpServerConfigurator AddStandaloneService(this IRtmpServerConfigurator configurator)
        {
            var services = configurator.Services;

            services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(Installer).Assembly));
            services.AddSingleton<IRtmpStreamManagerService, RtmpStreamManagerService>()
                    .AddSingleton<IRtmpStreamManagerApiService, RtmpStreamManagerApiService>();

            configurator.AddStreamEventHandler<RtmpServerStreamEventListener>();

            return configurator;
        }
    }
}
