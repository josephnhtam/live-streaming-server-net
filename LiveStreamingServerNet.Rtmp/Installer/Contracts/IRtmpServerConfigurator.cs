using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Rtmp.Installer.Contracts
{
    public interface IRtmpServerConfigurator
    {
        IServiceCollection Services { get; }
        IRtmpServerConfigurator ConfigureRtmpServer(Action<RtmpServerConfiguration>? configure);
        IRtmpServerConfigurator ConfigureMediaMessage(Action<MediaMessageConfiguration>? configure);
        IRtmpServerConfigurator AddAuthorizationHandler<TAuthorizationHandler>()
            where TAuthorizationHandler : class, IRtmpAuthorizationHandler;
        IRtmpServerConfigurator AddAuthorizationHandler(Func<IServiceProvider, IRtmpAuthorizationHandler> implmentationFactory);
    }
}
