using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer
{
    public static class HlsFilesInstaller
    {
        public static IApplicationBuilder UseHlsFiles(this IApplicationBuilder app, IServer liveStreamingServer, HlsServingOptions? options = null)
        {
            if (options == null)
                app.UseMiddleware<HlsFilesMiddleware>(liveStreamingServer);
            else
                app.UseMiddleware<HlsFilesMiddleware>(liveStreamingServer, Options.Create(options));

            return app;
        }

        public static IApplicationBuilder UseHlsFiles(this IApplicationBuilder app, HlsServingOptions? options = null)
        {
            if (options == null)
                app.UseMiddleware<HlsFilesMiddleware>();
            else
                app.UseMiddleware<HlsFilesMiddleware>(Options.Create(options));

            return app;
        }
    }
}
