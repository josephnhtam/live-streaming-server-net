using LiveStreamingServerNet.Rtmp.Client.Configurations;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Controllers
{
    internal class RtmpClientController : IRtmpClientController
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly IRtmpCommanderService _commander;
        private readonly IRtmpProtocolControlService _protocolControl;
        private readonly RtmpClientConfiguration _config;

        private int _connectOnce;

        public RtmpClientController(
            IRtmpClientContext clientContext,
            IRtmpChunkMessageSenderService chunkMessageSender,
            IRtmpCommanderService commander,
            IRtmpProtocolControlService protocolControl,
            IOptions<RtmpClientConfiguration> config)
        {
            _chunkMessageSender = chunkMessageSender;
            _commander = commander;
            _protocolControl = protocolControl;
            _config = config.Value;
        }

        public void Connect(string appName)
        {
            Connect(appName, new Dictionary<string, object>());
        }

        public void Connect(string appName, IDictionary<string, object> information)
        {
            if (Interlocked.CompareExchange(ref _connectOnce, 1, 0) == 1)
                throw new InvalidOperationException("Connect method can be called only once.");

            _protocolControl.SetChunkSize(_config.OutChunkSize);
            _protocolControl.WindowAcknowledgementSize(_config.WindowAcknowledgementSize);
            _commander.Connect(appName, information);
        }

        public void CreateStream()
        {
            _commander.CreateStream();
        }
    }
}
