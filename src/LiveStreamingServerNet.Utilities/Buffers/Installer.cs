using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public static class Installer
    {
        /// <summary>
        /// Adds the buffer pool service to the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBufferPool(this IServiceCollection services)
        {
            services.TryAddSingleton<IBufferPool>(svc =>
            {
                var config = svc.GetRequiredService<IOptions<BufferPoolConfiguration>>();
                return new BufferPool(config);
            });

            return services;
        }

        /// <summary>
        /// Adds the data buffer pool service to the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDataBufferPool(this IServiceCollection services)
        {
            services.TryAddSingleton<IDataBufferPool>(svc =>
            {
                var config = svc.GetRequiredService<IOptions<DataBufferPoolConfiguration>>();
                return new DataBufferPool(config);
            });

            return services;
        }
    }
}
