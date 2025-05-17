using LiveStreamingServerNet.StreamProcessor.Configurations;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    /// <summary>
    /// Provides extension methods for installing and configuring the on-demand stream capturer service.
    /// </summary>
    public static class OnDemandStreamCapturerInstaller
    {
        /// <summary>
        /// Adds the on-demand stream capturer service to the stream processing builder using the default configuration.
        /// </summary>
        /// <param name="builder">The stream processing builder to add services to.</param>
        /// <returns>The stream processing builder for method chaining.</returns>
        public static IStreamProcessingBuilder AddOnDemandStreamCapturer(this IStreamProcessingBuilder builder)
        {
            return AddOnDemandStreamCapturer(builder, null);
        }

        /// <summary>
        /// Adds the on-demand stream capturer service to the stream processing builder with an optional configuration.
        /// </summary>
        /// <param name="builder">The stream processing builder to add services to.</param>
        /// <param name="configure">Optional action to configure the on-demand stream capturer settings.</param>
        /// <returns>The stream processing builder for method chaining.</returns>
        public static IStreamProcessingBuilder AddOnDemandStreamCapturer(this IStreamProcessingBuilder builder, Action<OnDemandStreamCapturerConfiguration>? configure)
        {
            var services = builder.Services;

            if (configure != null)
                services.Configure(configure);

            services.TryAddSingleton<IOnDemandStreamCapturer, OnDemandStreamCapturer>();

            return builder;
        }
    }
}
