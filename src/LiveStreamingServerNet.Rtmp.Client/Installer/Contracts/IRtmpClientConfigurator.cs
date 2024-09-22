using LiveStreamingClientNet.Rtmp.Client.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Installer.Contracts
{
    public interface IRtmpClientConfigurator
    {
        IServiceCollection Services { get; }

        IRtmpClientConfigurator AddConnectionEventHandler<TConnectionEventHandler>()
            where TConnectionEventHandler : class, IRtmpClientConnectionEventHandler;

        IRtmpClientConfigurator AddConnectionEventHandler(Func<IServiceProvider, IRtmpClientConnectionEventHandler> implementationFactory);

        IRtmpClientConfigurator AddStreamEventHandler<TStreamEventHandler>()
            where TStreamEventHandler : class, IRtmpClientStreamEventHandler;

        IRtmpClientConfigurator AddStreamEventHandler(Func<IServiceProvider, IRtmpClientStreamEventHandler> implementationFactory);

    }
}