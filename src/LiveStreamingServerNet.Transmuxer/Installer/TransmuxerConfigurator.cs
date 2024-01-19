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

        public ITransmuxerConfigurator UseInputPathResolver<TInputPathResolver>()
            where TInputPathResolver : class, IInputPathResolver
        {
            Services.AddSingleton<IInputPathResolver, TInputPathResolver>();
            return this;
        }

        public ITransmuxerConfigurator UserOutputDirectoryPathResolver<TOutputDirectoryPathResolver>()
            where TOutputDirectoryPathResolver : class, IOutputDirectoryPathResolver
        {
            Services.AddSingleton<IOutputDirectoryPathResolver, TOutputDirectoryPathResolver>();
            return this;
        }
    }
}
