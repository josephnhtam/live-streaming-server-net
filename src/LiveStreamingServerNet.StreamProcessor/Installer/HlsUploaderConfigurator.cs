using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    internal class HlsUploaderConfigurator : IHlsUploaderConfigurator
    {
        public IServiceCollection Services { get; }

        public HlsUploaderConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IHlsUploaderConfigurator AddHlsStorageEventHandler<THlsStorageEventHandler>()
            where THlsStorageEventHandler : class, IHlsStorageEventHandler
        {
            Services.AddSingleton<IHlsStorageEventHandler, THlsStorageEventHandler>();
            return this;
        }

        public IHlsUploaderConfigurator AddHlsStorageEventHandler(Func<IServiceProvider, IHlsStorageEventHandler> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }

        public IHlsUploaderConfigurator AddHlsUploaderCondition<THlsUploaderCondition>()
            where THlsUploaderCondition : class, IHlsUploaderCondition
        {
            Services.AddSingleton<IHlsUploaderCondition, THlsUploaderCondition>();
            return this;
        }

        public IHlsUploaderConfigurator AddHlsUploaderCondition(Func<IServiceProvider, IHlsUploaderCondition> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }
    }
}
