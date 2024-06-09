using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    internal class StreamProcessingConfigurator : IStreamProcessingConfigurator
    {
        public IServiceCollection Services { get; }

        public StreamProcessingConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IStreamProcessingConfigurator UseInputPathResolver<TInputPathResolver>()
            where TInputPathResolver : class, IInputPathResolver
        {
            Services.AddSingleton<IInputPathResolver, TInputPathResolver>();
            return this;
        }

        public IStreamProcessingConfigurator UseInputPathResolver(Func<IServiceProvider, IInputPathResolver> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }

        public IStreamProcessingConfigurator AddStreamProcessorEventHandler<TStreamProcessorEventHandler>()
            where TStreamProcessorEventHandler : class, IStreamProcessorEventHandler
        {
            Services.AddSingleton<IStreamProcessorEventHandler, TStreamProcessorEventHandler>();
            return this;
        }

        public IStreamProcessingConfigurator AddStreamProcessorEventHandler(Func<IServiceProvider, IStreamProcessorEventHandler> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }
    }
}
