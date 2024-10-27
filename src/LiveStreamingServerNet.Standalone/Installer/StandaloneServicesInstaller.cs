using LiveStreamingServerNet.Rtmp.Server.Installer.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Services;
using LiveStreamingServerNet.Standalone.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Standalone.Installer
{
    /// <summary>
    /// Provides extension methods for installing RTMP server API services.
    /// </summary>
    public static class StandaloneServicesInstaller
    {
        /// <summary>
        /// Adds services required for standalone RTMP server API functionality.
        /// </summary>
        /// <param name="configurator">The RTMP server configurator</param>
        /// <returns>The configurator for method chaining</returns>
        public static IRtmpServerConfigurator AddStandaloneServices(this IRtmpServerConfigurator configurator)
        {
            var services = configurator.Services;

            services.AddSingleton<IRtmpStreamManagerApiService, RtmpStreamManagerApiService>();

            return configurator;
        }
    }
}
