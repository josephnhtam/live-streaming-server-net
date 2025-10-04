using LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using System.Net;

namespace LiveStreamingServerNet.AdaptiveHlsWithCustomFiltersDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLiveStreamingServer();

            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyHeader()
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                )
            );

            var app = builder.Build();

            app.UseCors();

            // Given that the scheme is https, the port is 7138, and the stream path is live/demo,
            // the HLS stream will be available at https://localhost:7138/live/demo/output.m3u8
            app.UseHlsFiles();

            await app.RunAsync();
        }

        private static IServiceCollection AddLiveStreamingServer(this IServiceCollection services)
        {
            return services.AddLiveStreamingServer(
                new IPEndPoint(IPAddress.Any, 1935),
                options => options
                    .Configure(options => options.EnableGopCaching = false)
                    .AddStreamProcessor()
                    .AddAdaptiveHlsTranscoder(configure =>
                        configure.ConfigureDefault(config =>
                        {
                            config.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                            config.FFprobePath = ExecutableFinder.FindExecutableFromPATH("ffprobe")!;

                            // Add an icon as the second input to the FFmpeg command
                            config.AdditionalInputs = [Path.Combine(Directory.GetCurrentDirectory(), "icon.png")];

                            // Add custom complex filters to scale the icon to different sizes
                            config.AdditionalComplexFilters =
                            [
                                "[1:v]scale=-2:64[icon360]",
                                "[1:v]scale=-2:85[icon480]",
                                "[1:v]scale=-2:128[icon720]",
                                "[1:v]scale=-2:128[icon720t]"
                            ];

                            // Add custom audio filters to reduce the volume by half
                            config.AudioFilters = ["volume=0.5"];

                            config.DownsamplingFilters =
                            [
                                new DownsamplingFilter(
                                    Name: "360p",
                                    Height: 360,
                                    MaxVideoBitrate: "600k",
                                    MaxAudioBitrate: "64k",
                                    // Overlay the icon360 on the video
                                    VideoFilter: ["[icon360]overlay"]
                                ),

                                new DownsamplingFilter(
                                    Name: "480p",
                                    Height: 480,
                                    MaxVideoBitrate: "1500k",
                                    MaxAudioBitrate: "128k",
                                    // Overlay the icon480 on the video
                                    VideoFilter: ["[icon480]overlay"]
                                ),

                                new DownsamplingFilter(
                                    Name: "720p",
                                    Height: 720,
                                    MaxVideoBitrate: "3000k",
                                    MaxAudioBitrate: "256k",
                                    // Overlay the icon720 on the video
                                    VideoFilter: ["[icon720]overlay"]
                                ),

                                new DownsamplingFilter(
                                    Name: "720p_rotated",
                                    Height: 720,
                                    MaxVideoBitrate: "3000k",
                                    MaxAudioBitrate: "256k",
                                    // Overlay the icon720t on the video, and rotate the video 90 degrees
                                    VideoFilter: ["[icon720t]overlay", "transpose=1"]
                                ),
                            ];
                        })
                    )
            );
        }
    }
}
