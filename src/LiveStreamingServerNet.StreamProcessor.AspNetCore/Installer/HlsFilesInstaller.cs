using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer
{
    /// <summary>
    /// Provides extension methods for serving HLS files in ASP.NET Core applications.
    /// </summary>
    public static class HlsFilesInstaller
    {
        /// <summary>
        /// Adds HLS file serving middleware with a live streaming server.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="liveStreamingServer">The live streaming server instance.</param>
        public static IApplicationBuilder UseHlsFiles(this IApplicationBuilder app, IServer liveStreamingServer)
            => app.UseHlsFiles(liveStreamingServer, null);

        /// <summary>
        /// Adds HLS file serving middleware with a live streaming server.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="liveStreamingServer">The live streaming server instance.</param>
        /// <param name="options">HLS serving options.</param>
        public static IApplicationBuilder UseHlsFiles(this IApplicationBuilder app, IServer liveStreamingServer, HlsServingOptions? options)
        {
            if (options == null)
                app.UseMiddleware<HlsFilesMiddleware>(liveStreamingServer);
            else
                app.UseMiddleware<HlsFilesMiddleware>(liveStreamingServer, Options.Create(options));

            return app;
        }

        /// <summary>
        /// Adds HLS file serving middleware.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public static IApplicationBuilder UseHlsFiles(this IApplicationBuilder app)
            => app.UseHlsFiles(options: null);

        /// <summary>
        /// Adds HLS file serving middleware.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="options">HLS serving options.</param>
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
