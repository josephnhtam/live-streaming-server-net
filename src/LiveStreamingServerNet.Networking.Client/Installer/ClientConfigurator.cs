using LiveStreamingServerNet.Networking.Client.Configurations;
using LiveStreamingServerNet.Networking.Client.Contracts;
using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Networking.Client.Installer
{
    internal class ClientConfigurator : IClientConfigurator
    {
        public IServiceCollection Services { get; }

        public ClientConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IClientConfigurator ConfigureNetwork(Action<NetworkConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }

        public IClientConfigurator ConfigureSecurity(Action<SecurityConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }

        public IClientConfigurator ConfigureDataBufferPool(Action<DataBufferPoolConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }

        public IClientConfigurator ConfigureBufferPool(Action<BufferPoolConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }

        public IClientConfigurator AddServerCertificateValidator<TServerCertificateValidator>()
            where TServerCertificateValidator : class, IServerCertificateValidator
        {
            Services.TryAddSingleton<IServerCertificateValidator, TServerCertificateValidator>();
            return this;
        }

        public IClientConfigurator AddServerCertificateValidator(Func<IServiceProvider, IServerCertificateValidator> factory)
        {
            Services.TryAddSingleton(factory);
            return this;
        }

        public IClientConfigurator AddClientEventHandler<TServerEventHandler>() where TServerEventHandler : class, IClientEventHandler
        {
            Services.AddSingleton<IClientEventHandler, TServerEventHandler>();
            return this;
        }

        public IClientConfigurator AddClientEventHandler<TServerEventHandler>(Func<IServiceProvider, IClientEventHandler> factory)
        {
            Services.AddSingleton(factory);
            return this;
        }
    }
}
