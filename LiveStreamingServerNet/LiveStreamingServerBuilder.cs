using LiveStreamingServerNet.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Configurations;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet
{
    public class LiveStreamingServerBuilder : ILiveStreamingServerBuilder
    {
        private readonly ServiceCollection _services;

        private LiveStreamingServerBuilder()
        {
            _services = new ServiceCollection();

            AddCore();
            AddRtmpServer();
        }

        public static ILiveStreamingServerBuilder Create()
        {
            return new LiveStreamingServerBuilder();
        }

        private void AddCore()
        {
            _services.AddOptions()
                     .AddLogging();
        }

        private void AddRtmpServer()
        {
            _services.AddRtmpServer();
        }

        public ILiveStreamingServerBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            _services.AddLogging(configure);
            return this;
        }

        public ILiveStreamingServerBuilder ConfigureRtmpServer(Action<RtmpServerConfiguration> configure)
        {
            _services.Configure(configure);
            return this;
        }

        public ILiveStreamingServerBuilder ConfigureMediaMessage(Action<MediaMessageConfiguration> configure)
        {
            _services.Configure(configure);
            return this;
        }

        public ILiveStreamingServerBuilder ConfigureNetBufferPool(Action<NetBufferPoolConfiguration> configure)
        {
            _services.Configure(configure);
            return this;
        }

        public IServer Build()
        {
            var provider = _services.BuildServiceProvider();
            return provider.GetRequiredService<IServer>();
        }
    }
}
