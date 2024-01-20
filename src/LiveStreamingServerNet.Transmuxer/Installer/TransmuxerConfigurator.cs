using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer
{
    public class TransmuxerConfigurator : ITransmuxerConfigurator
    {
        public IServiceCollection Services { get; }

        public TransmuxerConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public ITransmuxerConfigurator Configure(Action<RemuxingConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }

        public ITransmuxerConfigurator UseInputPathResolver<TInputPathResolver>()
            where TInputPathResolver : class, IInputPathResolver
        {
            Services.AddSingleton<IInputPathResolver, TInputPathResolver>();
            return this;
        }

        public ITransmuxerConfigurator UseInputPathResolver(Func<IServiceProvider, IInputPathResolver> implmentationFactory)
        {
            Services.AddSingleton(implmentationFactory);
            return this;
        }

        public ITransmuxerConfigurator UserOutputDirectoryPathResolver<TOutputDirectoryPathResolver>()
            where TOutputDirectoryPathResolver : class, IOutputDirectoryPathResolver
        {
            Services.AddSingleton<IOutputDirectoryPathResolver, TOutputDirectoryPathResolver>();
            return this;
        }

        public ITransmuxerConfigurator UserOutputDirectoryPathResolver(Func<IServiceProvider, IOutputDirectoryPathResolver> implmentationFactory)
        {
            Services.AddSingleton(implmentationFactory);
            return this;
        }

        public ITransmuxerConfigurator AddTransmuxerEventHandler<TTransmuxerEventHandler>()
            where TTransmuxerEventHandler : class, ITransmuxerEventHandler
        {
            Services.AddSingleton<ITransmuxerEventHandler, TTransmuxerEventHandler>();
            return this;
        }

        public ITransmuxerConfigurator AddTransmuxerEventHandler(Func<IServiceProvider, ITransmuxerEventHandler> implmentationFactory)
        {
            Services.AddSingleton(implmentationFactory);
            return this;
        }
    }
}
