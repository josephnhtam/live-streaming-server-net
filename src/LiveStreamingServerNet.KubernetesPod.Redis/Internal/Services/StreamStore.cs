using LiveStreamingServerNet.KubernetesPod.Configurations;
using LiveStreamingServerNet.KubernetesPod.Redis.Internal.Logging;
using LiveStreamingServerNet.KubernetesPod.Redis.Internal.Services.Contracts;
using LiveStreamingServerNet.KubernetesPod.StreamRegistration;
using LiveStreamingServerNet.KubernetesPod.StreamRegistration.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace LiveStreamingServerNet.KubernetesPod.Redis.Internal.Services
{
    internal class StreamStore : IStreamStore
    {
        private readonly IDatabase _database;
        private readonly IStreamKeyProvider _streamKeyProvider;
        private readonly ILogger _logger;
        private readonly StreamRegistryConfiguration _config;
        private readonly ConcurrentDictionary<string, StreamInfo> _streamInfos;

        public StreamStore(IDatabase database, IStreamKeyProvider streamKeyProvider, ILogger<StreamStore> logger, IOptions<StreamRegistryConfiguration> config)
        {
            _database = database;
            _streamKeyProvider = streamKeyProvider;
            _logger = logger;
            _config = config.Value;

            _streamInfos = new ConcurrentDictionary<string, StreamInfo>();
        }

        private string ResolveStreamKey(string streamPath)
        {
            return _streamKeyProvider.ResolveStreamKey(streamPath);
        }

        public async Task<bool> IsStreamRegisteredAsync(string streamPath, CancellationToken cancellationToken = default)
        {
            var key = ResolveStreamKey(streamPath);
            return await _database.KeyExistsAsync(key);
        }

        public async Task<StreamRegistrationResult> RegisterStreamAsync(
            IClientInfo client,
            string podNamespace,
            string podName,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var key = ResolveStreamKey(streamPath);

                var streamInfo = new StreamInfo(
                    client.ClientId,
                    client.LocalEndPoint.ToString() ?? "Unknown",
                    client.RemoteEndPoint.ToString() ?? "Unknown",
                    podNamespace,
                    podName,
                    streamPath,
                    streamArguments);

                var set = await _database.StringSetAsync(
                    key,
                    JsonSerializer.Serialize(streamInfo),
                    expiry: _config.KeepaliveTimeout + _config.KeepaliveTolerance,
                    when: When.NotExists
                );

                if (set)
                    _streamInfos[key] = streamInfo;

                return set ?
                    StreamRegistrationResult.Success() :
                    StreamRegistrationResult.Failure("Stream is already registered.");
            }
            catch (Exception ex)
            {
                _logger.RegisteringStreamError(streamPath, ex);
                return StreamRegistrationResult.Failure("An error occurred during registering stream.");
            }
        }

        public async Task<StreamRevalidationResult> RevalidateStreamAsync(string streamPath, CancellationToken cancellationToken = default)
        {
            try
            {
                var key = ResolveStreamKey(streamPath);

                var value = await _database.StringGetAsync(key);

                if (!value.HasValue)
                    return StreamRevalidationResult.Failure(false, "Stream not found.");

                if (!_streamInfos.TryGetValue(key, out var localStreamInfo))
                    return StreamRevalidationResult.Failure(false, "Stream not found in local cache.");

                try
                {
                    var streamInfo = JsonSerializer.Deserialize<StreamInfo>(value.ToString());

                    if (streamInfo == null)
                    {
                        return StreamRevalidationResult.Failure(false, "Failed to deserialize stream info.");
                    }

                    if (streamInfo.ClientId != localStreamInfo.ClientId ||
                        streamInfo.PodNamespace != localStreamInfo.PodNamespace ||
                        streamInfo.PodName != localStreamInfo.PodName)
                        return StreamRevalidationResult.Failure(false, "Stream has been moved to another pod.");
                }
                catch (Exception ex)
                {
                    _logger.DeserializingStreamInfoError(streamPath, ex);
                    return StreamRevalidationResult.Failure(false, "Failed to deserialize stream info.");
                }

                var transaction = _database.CreateTransaction();

                transaction.AddCondition(Condition.StringEqual(key, value));
                _ = transaction.KeyExpireAsync(key, _config.KeepaliveTimeout + _config.KeepaliveTolerance);

                var success = await transaction.ExecuteAsync();

                return success ?
                    StreamRevalidationResult.Success() :
                    StreamRevalidationResult.Failure(false, "Stream has been modified by another process.");
            }
            catch (Exception ex)
            {
                _logger.RevalidatingStreamError(streamPath, ex);
                return StreamRevalidationResult.Failure(ex is RedisException, "An error occurred during revalidating the stream.");
            }
        }

        public async Task UnregsiterStreamAsync(string streamPath, CancellationToken cancellationToken = default)
        {
            try
            {
                var key = ResolveStreamKey(streamPath);

                var value = await _database.StringGetAsync(key);

                if (!value.HasValue || !_streamInfos.TryGetValue(key, out var localStreamInfo))
                    return;

                try
                {
                    var streamInfo = JsonSerializer.Deserialize<StreamInfo>(value.ToString());

                    if (streamInfo == null)
                        return;

                    if (streamInfo.ClientId != localStreamInfo.ClientId ||
                        streamInfo.PodNamespace != localStreamInfo.PodNamespace ||
                        streamInfo.PodName != localStreamInfo.PodName)
                        return;
                }
                catch (Exception ex)
                {
                    _logger.DeserializingStreamInfoError(streamPath, ex);
                    return;
                }

                var transaction = _database.CreateTransaction();

                transaction.AddCondition(Condition.StringEqual(key, value));
                _ = transaction.KeyDeleteAsync(key);

                await transaction.ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.UnregisteringStreamError(streamPath, ex);
            }
        }
    }
}
