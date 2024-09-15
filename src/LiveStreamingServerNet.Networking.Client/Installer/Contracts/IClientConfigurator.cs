using LiveStreamingServerNet.Networking.Client.Configurations;
using LiveStreamingServerNet.Networking.Client.Contracts;
using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Networking.Client.Installer.Contracts
{
    public interface IClientConfigurator
    {
        IServiceCollection Services { get; }

        IClientConfigurator ConfigureNetwork(Action<NetworkConfiguration>? configure);

        IClientConfigurator ConfigureSecurity(Action<SecurityConfiguration>? configure);

        IClientConfigurator ConfigureDataBufferPool(Action<DataBufferPoolConfiguration>? configure);

        IClientConfigurator ConfigureBufferPool(Action<BufferPoolConfiguration>? configure);

        IClientConfigurator AddServerCertificateValidator<TServerCertificateValidator>()
            where TServerCertificateValidator : class, IServerCertificateValidator;

        IClientConfigurator AddServerCertificateValidator(Func<IServiceProvider, IServerCertificateValidator> factory);

        IClientConfigurator AddClientEventHandler<TServerEventHandler>()
            where TServerEventHandler : class, IClientEventHandler;

        IClientConfigurator AddClientEventHandler<TServerEventHandler>(Func<IServiceProvider, IClientEventHandler> factory);
    }
}
