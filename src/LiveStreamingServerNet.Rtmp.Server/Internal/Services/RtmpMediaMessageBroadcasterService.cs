using LiveStreamingServerNet.Networking.Exceptions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.MediaPackageDiscarding.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpMediaMessageBroadcasterService : IRtmpMediaMessageBroadcasterService
    {
        private readonly IRtmpChunkMessageWriterService _chunkMessageWriter;
        private readonly IRtmpMediaMessageInterceptionService _interception;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IMediaPackageDiscarderFactory _mediaPackageDiscarderFactory;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<IRtmpClientSessionContext, ClientMediaContext> _clientMediaContexts = new();
        private readonly ConcurrentDictionary<IRtmpClientSessionContext, Task> _clientTasks = new();

        public RtmpMediaMessageBroadcasterService(
            IRtmpChunkMessageWriterService chunkMessageWriter,
            IRtmpMediaMessageInterceptionService interception,
            IDataBufferPool dataBufferPool,
            IMediaPackageDiscarderFactory mediaPackageDiscarderFactory,
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpMediaMessageBroadcasterService> logger)
        {
            _chunkMessageWriter = chunkMessageWriter;
            _interception = interception;
            _dataBufferPool = dataBufferPool;
            _mediaPackageDiscarderFactory = mediaPackageDiscarderFactory;
            _config = config.Value;
            _logger = logger;
        }

        private ClientMediaContext? GetMediaContext(IRtmpClientSessionContext clientContext)
        {
            return _clientMediaContexts.GetValueOrDefault(clientContext);
        }

        public void RegisterClient(IRtmpClientSessionContext clientContext)
        {
            _clientMediaContexts[clientContext] = new ClientMediaContext(clientContext, _mediaPackageDiscarderFactory);

            var clientTask = Task.Run(() => ClientTask(clientContext));
            _clientTasks[clientContext] = clientTask;
            _ = clientTask.ContinueWith(_ => _clientTasks.TryRemove(clientContext, out var _));
        }

        public void UnregisterClient(IRtmpClientSessionContext clientContext)
        {
            if (_clientMediaContexts.TryRemove(clientContext, out var context))
            {
                context.Stop();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_clientTasks.Values);
        }

        public async ValueTask BroadcastMediaMessageAsync(
            IRtmpPublishStreamContext publishStreamContext,
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts,
            MediaType mediaType,
            uint timestamp,
            bool isSkippable,
            IDataBuffer payloadBuffer)
        {
            await _interception.ReceiveMediaMessageAsync(publishStreamContext.StreamPath, mediaType, payloadBuffer, timestamp, isSkippable);

            subscribeStreamContexts = subscribeStreamContexts.Where((subscriber) => FilterSubscribers(subscriber, isSkippable)).ToList();

            if (subscribeStreamContexts.Any())
                EnqueueMediaPackages(subscribeStreamContexts, mediaType, payloadBuffer, timestamp, isSkippable);

            bool FilterSubscribers(IRtmpSubscribeStreamContext subscribeStreamContext, bool isSkippable)
            {
                if (!isSkippable)
                    return true;

                switch (mediaType)
                {
                    case MediaType.Audio:
                        if (!subscribeStreamContext.IsReceivingAudio)
                            return false;
                        break;
                    case MediaType.Video:
                        if (!subscribeStreamContext.IsReceivingVideo)
                            return false;
                        break;
                }

                return true;
            }
        }

        private void EnqueueMediaPackages(
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts,
            MediaType type,
            IDataBuffer payloadBuffer,
            uint timestamp,
            bool isSkippable)
        {
            foreach (var subscribersGroup in subscribeStreamContexts.GroupBy(x =>
                (x.StreamContext.StreamId,
                 ChunkStreamId: type == MediaType.Video ? x.VideoChunkStreamId : x.AudioChunkStreamId,
                 x.StreamContext.ClientContext.OutChunkSize)))
            {
                var streamId = subscribersGroup.Key.StreamId;
                var chunkStreamId = subscribersGroup.Key.ChunkStreamId;
                var outChunkSize = subscribersGroup.Key.OutChunkSize;
                var subscribers = subscribersGroup.ToList();

                var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);

                var messageHeader = new RtmpChunkMessageHeaderType0(
                    timestamp,
                    payloadBuffer.Size,
                    type == MediaType.Video ?
                    RtmpMessageType.VideoMessage :
                    RtmpMessageType.AudioMessage,
                    streamId);

                var tempBuffer = _dataBufferPool.Obtain();

                try
                {
                    _chunkMessageWriter.Write(tempBuffer, basicHeader, messageHeader, payloadBuffer.MoveTo(0), outChunkSize);

                    var rentedBuffer = tempBuffer.ToRentedBuffer(subscribers.Count);

                    foreach (var subscriber in subscribers)
                    {
                        var mediaPackage = new ClientMediaPackage(subscriber, rentedBuffer, isSkippable, timestamp, type);

                        var mediaContext = GetMediaContext(subscriber.StreamContext.ClientContext);
                        if (mediaContext == null || !mediaContext.AddPackage(ref mediaPackage))
                            rentedBuffer.Unclaim();
                    }
                }
                finally
                {
                    _dataBufferPool.Recycle(tempBuffer);
                }
            }
        }

        private async Task ClientTask(IRtmpClientSessionContext clientContext)
        {
            var context = _clientMediaContexts.GetValueOrDefault(clientContext);

            if (context == null)
                return;

            var cancellation = context.CancellationToken;
            var subscriptionInitialized = false;

            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var package = await context.ReadPackageAsync(cancellation);

                    try
                    {
                        if (!subscriptionInitialized)
                        {
                            await package.StreamContext.UntilInitializationCompleteAsync();
                            subscriptionInitialized = true;
                        }

                        await SendPackageAsync(context, clientContext, package, cancellation);
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
                    catch (BufferSendingException) when (!context.ClientContext.Client.IsConnected) { }
                    catch (Exception ex)
                    {
                        _logger.FailedToSendMediaMessage(clientContext.Client.Id, ex);
                    }
                    finally
                    {
                        package.RentedPayload.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            catch (ChannelClosedException) { }

            while (context.ReadPackage(out var package))
            {
                package.RentedPayload.Unclaim();
            }
        }

        private async Task SendPackageAsync(ClientMediaContext context, IRtmpClientSessionContext clientContext, ClientMediaPackage package, CancellationToken cancellation)
        {
            var streamContext = package.StreamContext;

            if (!streamContext.UpdateTimestamp(package.Timestamp, package.MediaType) && package.IsSkippable)
                return;

            if (_config.MediaPackageBatchWindow > TimeSpan.Zero)
                await BatchAndSendPackageAsync(context, clientContext, package, cancellation);
            else
                await clientContext.Client.SendAsync(package.RentedPayload);
        }

        private async Task BatchAndSendPackageAsync(ClientMediaContext context, IRtmpClientSessionContext clientContext, ClientMediaPackage firstPackage, CancellationToken cancellation)
        {
            await Task.Delay(_config.MediaPackageBatchWindow, cancellation);

            var packages = new List<ClientMediaPackage>() { firstPackage };

            while (context.ReadPackage(out var package))
            {
                if (package.StreamContext.UpdateTimestamp(package.Timestamp, package.MediaType) || !package.IsSkippable)
                    packages.Add(package);
                else
                    package.RentedPayload.Unclaim();
            }

            await FlushAsync(clientContext, packages);
        }

        private async Task FlushAsync(IRtmpClientSessionContext clientContext, List<ClientMediaPackage> packages)
        {
            try
            {
                var tempBuffer = new RentedBuffer(_dataBufferPool.BufferPool, packages.Sum(x => x.RentedPayload.Size));

                try
                {
                    for (int i = 0, offset = 0; i < packages.Count; i++)
                    {
                        var package = packages[i];
                        var buffer = package.RentedPayload.Buffer;
                        var bufferSize = package.RentedPayload.Size;

                        buffer.AsSpan(0, bufferSize).CopyTo(tempBuffer.Buffer.AsSpan(offset, bufferSize));
                        offset += bufferSize;
                    }

                    await clientContext.Client.SendAsync(tempBuffer);
                }
                finally
                {
                    tempBuffer.Unclaim();
                }
            }
            finally
            {
                for (int i = 1; i < packages.Count; i++)
                    packages[i].RentedPayload.Unclaim();
            }
        }

        private class ClientMediaContext
        {
            public readonly IRtmpClientSessionContext ClientContext;
            public readonly CancellationToken CancellationToken;
            public long OutstandingPackagesSize => _outstandingPackagesSize;
            public long OutstandingPackagesCount => _outstandingPackageCount;

            private readonly IMediaPackageDiscarder _mediaPackageDiscarder;
            private readonly Channel<ClientMediaPackage> _packageChannel;
            private readonly CancellationTokenSource _cts;

            private long _outstandingPackagesSize;
            private long _outstandingPackageCount;

            public ClientMediaContext(IRtmpClientSessionContext clientContext, IMediaPackageDiscarderFactory mediaPackageDiscarderFactory)
            {
                ClientContext = clientContext;
                _mediaPackageDiscarder = mediaPackageDiscarderFactory.Create(clientContext.Client.Id);

                _packageChannel = Channel.CreateUnbounded<ClientMediaPackage>(
                    new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = true });
                _cts = new CancellationTokenSource();
                CancellationToken = _cts.Token;
            }

            public void Stop()
            {
                _packageChannel.Writer.Complete();
                _cts.Cancel();
            }

            public bool AddPackage(ref ClientMediaPackage package)
            {
                if (ShouldSkipPackage(this, package.IsSkippable))
                {
                    return false;
                }

                if (!_packageChannel.Writer.TryWrite(package))
                {
                    return false;
                }

                Interlocked.Add(ref _outstandingPackagesSize, package.RentedPayload.Size);
                Interlocked.Increment(ref _outstandingPackageCount);
                return true;
            }

            public async ValueTask<ClientMediaPackage> ReadPackageAsync(CancellationToken cancellation)
            {
                var package = await _packageChannel.Reader.ReadAsync(cancellation);
                Interlocked.Add(ref _outstandingPackagesSize, -package.RentedPayload.Size);
                Interlocked.Decrement(ref _outstandingPackageCount);
                return package;
            }

            public bool ReadPackage(out ClientMediaPackage package)
            {
                var result = _packageChannel.Reader.TryRead(out package);

                if (result)
                {
                    Interlocked.Add(ref _outstandingPackagesSize, -package.RentedPayload.Size);
                    Interlocked.Decrement(ref _outstandingPackageCount);
                }

                return result;
            }

            private bool ShouldSkipPackage(ClientMediaContext context, bool isSkippable)
            {
                return _mediaPackageDiscarder.ShouldDiscardMediaPackage(
                    isSkippable, context.OutstandingPackagesSize, context.OutstandingPackagesCount);
            }
        }

        private record struct ClientMediaPackage(IRtmpSubscribeStreamContext StreamContext, IRentedBuffer RentedPayload, bool IsSkippable, uint Timestamp, MediaType MediaType);
    }
}
