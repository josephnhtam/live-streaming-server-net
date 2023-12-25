using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Contracts;
using LiveStreamingServer.Rtmp.Core;
using LiveStreamingServer.Rtmp.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp
{
    public class RtmpServerBuilder : IRtmpServerBuilder
    {
        private readonly ServiceCollection _services;

        private RtmpServerBuilder()
        {
            _services = new ServiceCollection();

            _services.AddLogging();

            _services.AddSingleton<IServer, Server>()
                     .AddTransient<IClientPeer, ClientPeer>()
                     .AddSingleton<INetBufferPool, NetBufferPool>()
                     .AddSingleton<IClientPeerHandlerFactory, RtmpClientPeerHandlerFactory>()
                     .AddTransient<IRtmpClientPeerHandler, RtmpClientPeerHandler>();

            _services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssemblyContaining<RtmpClientPeerHandler>();
            });

            _services.AddSingleton<IRtmpServerContext, RtmpServerContext>();
        }

        public static IRtmpServerBuilder Create()
        {
            return new RtmpServerBuilder();
        }

        public IRtmpServerBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging)
        {
            _services.AddLogging(configureLogging);
            return this;
        }

        public IServer Build()
        {
            var provider = _services.BuildServiceProvider();
            return provider.GetRequiredService<IServer>();
        }
    }
}
