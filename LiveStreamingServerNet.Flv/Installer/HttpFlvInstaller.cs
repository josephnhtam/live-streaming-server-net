using LiveStreamingServerNet.Flv.Internal.Middlewares;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.AspNetCore.Builder;

namespace LiveStreamingServerNet.Flv.Installer
{
    public static class HttpFlvInstaller
    {
        public static IRtmpServerConfigurator AddHttpFlv(this IRtmpServerConfigurator configurator)
        {
            return configurator;
        }

        public static void UseHttpFlv(this WebApplication webApplication)
        {
            webApplication.UseMiddleware<HttpFlvMiddleware>();
        }
    }
}
