using LiveStreamingServerNet.AdminPanelUI;
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Standalone;
using LiveStreamingServerNet.Standalone.Installer;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer;
using LiveStreamingServerNet.StreamProcessor.Installer;
using System.Net;

namespace LiveStreamingServerNet.StandaloneDemo
{
    public static class Program
    {
        private const bool UseHttpFlvPreview = true;
        private const bool UseHlsPreview = true;

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLiveStreamingServer(UseHttpFlvPreview, UseHlsPreview);

            var app = builder.Build();

            if (UseHttpFlvPreview)
            {
                app.UseHttpFlv();
            }

            if (UseHlsPreview)
            {
                app.UseHlsFiles();
            }

            app.MapStandaloneServerApiEndPoints();
            app.UseAdminPanelUI(new AdminPanelUIOptions
            {
                // The Admin Panel UI will be available at https://localhost:7000/ui
                BasePath = "/ui",

                // The Admin Panel UI will access HTTP-FLV streams at https://localhost:7000/{streamPath}.flv
                HasHttpFlvPreview = UseHttpFlvPreview,
                HttpFlvUriPattern = "{streamPath}.flv",

                // The Admin Panel UI will access HLS streams at https://localhost:7000/{streamPath}/output.m3u8
                HasHlsPreview = UseHlsPreview,
                HlsUriPattern = "{streamPath}/output.m3u8"
            });

            await app.RunAsync();
        }

        private static IServiceCollection AddLiveStreamingServer(
            this IServiceCollection services, bool useHttpFlvPreview, bool useHlsPreview)
        {
            return services.AddLiveStreamingServer(
                new IPEndPoint(IPAddress.Any, 1935),
                options =>
                {
                    options.AddStandaloneServices();
                    options.AddBitrateTracking();

                    if (useHttpFlvPreview)
                    {
                        options.AddFlv();
                    }

                    if (useHlsPreview)
                    {
                        options.AddStreamProcessor()
                            .AddHlsTransmuxer();
                    }
                }
            );
        }
    }
}
