using Amazon.S3;
using LiveStreamingServerNet.Transmuxer.AmazonS3.Configurations;
using LiveStreamingServerNet.Transmuxer.AmazonS3.Internal;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Transmuxer.AmazonS3.Installer
{
    public static class HlsAmazonS3Installer
    {
        public static IHlsUploaderConfigurator AddAmazonS3(
            this IHlsUploaderConfigurator configurator,
            IAmazonS3 s3Client,
            string bucket,
            Action<HlsAmazonS3Configuration>? configure = null)
        {
            var services = configurator.Services;

            var config = new HlsAmazonS3Configuration();
            configure?.Invoke(config);

            services.AddSingleton<IHlsStorageAdapter>(svc =>
                new HlsAmazonS3Adapter(
                    s3Client,
                    bucket,
                    config,
                    svc.GetRequiredService<ILogger<HlsAmazonS3Adapter>>()
                )
            );

            return configurator;
        }
    }
}
