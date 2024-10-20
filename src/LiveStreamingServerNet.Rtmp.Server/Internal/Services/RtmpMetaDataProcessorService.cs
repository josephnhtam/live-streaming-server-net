using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpMetaDataProcessorService : IRtmpMetaDataProcessorService
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCacherService _cacher;
        private readonly ILogger _logger;

        public RtmpMetaDataProcessorService(
            IRtmpStreamManagerService streamManager,
            IRtmpCacherService cacher,
            ILogger<RtmpMetaDataProcessorService> logger)
        {
            _streamManager = streamManager;
            _cacher = cacher;
            _logger = logger;
        }

        public async ValueTask<bool> ProcessMetaDataAsync(IRtmpPublishStreamContext publishStreamContext, uint timestamp, IReadOnlyDictionary<string, object> metaData)
        {
            await CacheMetaDataAsync(publishStreamContext, metaData);
            BroadcastMetaDataToSubscribers(publishStreamContext);
            return true;
        }

        private async Task CacheMetaDataAsync(IRtmpPublishStreamContext publishStreamContext, IReadOnlyDictionary<string, object> metaData)
        {
            await _cacher.CacheStreamMetaDataAsync(publishStreamContext, metaData);
        }

        private void BroadcastMetaDataToSubscribers(IRtmpPublishStreamContext publishStreamContext)
        {
            var subscribeStreamContexts = _streamManager.GetSubscribeStreamContexts(publishStreamContext.StreamPath);

            _cacher.SendCachedStreamMetaDataMessage(
                subscribeStreamContexts,
                publishStreamContext);
        }
    }
}
