using LiveStreamingServerNet.Networking.Exceptions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;
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
        private readonly IMediaPacketDiscarderFactory _mediaPacketDiscarderFactory;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<IRtmpClientSessionContext, ClientMediaContext> _clientMediaContexts = new();
        private readonly ConcurrentDictionary<IRtmpClientSessionContext, Task> _clientTasks = new();

        public RtmpMediaMessageBroadcasterService(
            IRtmpChunkMessageWriterService chunkMessageWriter,
            IRtmpMediaMessageInterceptionService interception,
            IDataBufferPool dataBufferPool,
            IMediaPacketDiscarderFactory mediaPacketDiscarderFactory,
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpMediaMessageBroadcasterService> logger)
        {
            _chunkMessageWriter = chunkMessageWriter;
            _interception = interception;
            _dataBufferPool = dataBufferPool;
            _mediaPacketDiscarderFactory = mediaPacketDiscarderFactory;
            _config = config.Value;
            _logger = logger;
        }

        private ClientMediaContext? GetMediaContext(IRtmpClientSessionContext clientContext)
        {
            return _clientMediaContexts.GetValueOrDefault(clientContext);
        }

        public void RegisterClient(IRtmpClientSessionContext clientContext)
        {
            _clientMediaContexts[clientContext] = new ClientMediaContext(clientContext, _mediaPacketDiscarderFactory);

            var clientTask = Task.Run(() => ClientTask(clientContext));
            _clientTasks[clientContext] = clientTask;
            _ = clientTask.ContinueWith(_ =>
                _clientTasks.TryRemove(clientContext, out var _), TaskContinuationOptions.ExecuteSynchronously);
        }

        public async ValueTask UnregisterClientAsync(IRtmpClientSessionContext clientContext)
        {
            if (_clientMediaContexts.TryRemove(clientContext, out var context))
            {
                context.Stop();
                await context.UntilCompleteAsync();
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
            await _interception.ReceiveMediaMessageAsync(publishStreamContext, mediaType, payloadBuffer, timestamp, isSkippable);

            subscribeStreamContexts = subscribeStreamContexts.Where((subscriber) => FilterSubscribers(subscriber, isSkippable)).ToList();

            if (subscribeStreamContexts.Any())
                EnqueueMediaPackets(subscribeStreamContexts, mediaType, payloadBuffer, timestamp + publishStreamContext.TimestampOffset, isSkippable);

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

        private void EnqueueMediaPackets(
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
                        var mediaPacket = new ClientMediaPacket(subscriber, rentedBuffer, isSkippable, timestamp, type);

                        var mediaContext = GetMediaContext(subscriber.StreamContext.ClientContext);
                        if (mediaContext == null || !mediaContext.AddPacket(ref mediaPacket))
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
                    var packet = await context.ReadPacketAsync(cancellation);

                    try
                    {
                        if (!subscriptionInitialized)
                        {
                            await packet.StreamContext.UntilInitializationCompleteAsync(cancellation);
                            subscriptionInitialized = true;
                        }

                        await SendPacketAsync(context, clientContext, packet, cancellation);
                    }
                    finally
                    {
                        packet.RentedPayload.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            catch (BufferSendingException) when (!context.ClientContext.Client.IsConnected) { }
            catch (Exception ex)
            {
                _logger.FailedToSendMediaMessage(clientContext.Client.Id, ex);
            }

            clientContext.Client.Disconnect();
            context.Cleanup();
            context.Complete();
        }

        private async Task SendPacketAsync(ClientMediaContext context, IRtmpClientSessionContext clientContext, ClientMediaPacket packet, CancellationToken cancellation)
        {
            var streamContext = packet.StreamContext;

            if (!streamContext.UpdateTimestamp(packet.Timestamp, packet.MediaType) && packet.IsSkippable)
                return;

            if (_config.MediaPacketBatchWindow > TimeSpan.Zero)
                await BatchAndSendPacketAsync(context, clientContext, packet, cancellation);
            else
                await clientContext.Client.SendAsync(packet.RentedPayload);
        }

        private async Task BatchAndSendPacketAsync(ClientMediaContext context, IRtmpClientSessionContext clientContext, ClientMediaPacket firstPacket, CancellationToken cancellation)
        {
            await Task.Delay(_config.MediaPacketBatchWindow, cancellation);

            var packets = new List<ClientMediaPacket>() { firstPacket };

            while (context.ReadPacket(out var packet))
            {
                if (packet.StreamContext.UpdateTimestamp(packet.Timestamp, packet.MediaType) || !packet.IsSkippable)
                    packets.Add(packet);
                else
                    packet.RentedPayload.Unclaim();
            }

            await FlushAsync(clientContext, packets);
        }

        private async Task FlushAsync(IRtmpClientSessionContext clientContext, List<ClientMediaPacket> packets)
        {
            try
            {
                var tempBuffer = new RentedBuffer(_dataBufferPool.BufferPool, packets.Sum(x => x.RentedPayload.Size));

                try
                {
                    for (int i = 0, offset = 0; i < packets.Count; i++)
                    {
                        var packet = packets[i];
                        var buffer = packet.RentedPayload.Buffer;
                        var bufferSize = packet.RentedPayload.Size;

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
                for (int i = 1; i < packets.Count; i++)
                    packets[i].RentedPayload.Unclaim();
            }
        }

        private class ClientMediaContext
        {
            public readonly IRtmpClientSessionContext ClientContext;
            public readonly CancellationToken CancellationToken;

            private readonly IPacketDiscarder _mediaPacketDiscarder;
            private readonly Channel<ClientMediaPacket> _packetChannel;
            private readonly CancellationTokenSource _cts;
            private readonly TaskCompletionSource _tcs;

            private long _outstandingPacketsSize;
            private long _outstandingPacketCount;

            public long OutstandingPacketsSize => _outstandingPacketsSize;
            public long OutstandingPacketsCount => _outstandingPacketCount;


            public ClientMediaContext(IRtmpClientSessionContext clientContext, IMediaPacketDiscarderFactory mediaPacketDiscarderFactory)
            {
                _mediaPacketDiscarder = mediaPacketDiscarderFactory.Create(clientContext.Client.Id);

                _packetChannel = Channel.CreateUnbounded<ClientMediaPacket>(
                    new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = true });

                _cts = new CancellationTokenSource();
                _tcs = new TaskCompletionSource();

                ClientContext = clientContext;
                CancellationToken = _cts.Token;
            }

            public void Stop()
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            public void Cleanup()
            {
                _packetChannel.Writer.Complete();

                while (ReadPacket(out var packet))
                {
                    packet.RentedPayload.Unclaim();
                }
            }

            public bool AddPacket(ref ClientMediaPacket packet)
            {
                if (ShouldSkipPacket(this, packet.IsSkippable))
                {
                    return false;
                }

                if (!_packetChannel.Writer.TryWrite(packet))
                {
                    return false;
                }

                Interlocked.Add(ref _outstandingPacketsSize, packet.RentedPayload.Size);
                Interlocked.Increment(ref _outstandingPacketCount);
                return true;
            }

            public async ValueTask<ClientMediaPacket> ReadPacketAsync(CancellationToken cancellation)
            {
                var packet = await _packetChannel.Reader.ReadAsync(cancellation);
                Interlocked.Add(ref _outstandingPacketsSize, -packet.RentedPayload.Size);
                Interlocked.Decrement(ref _outstandingPacketCount);
                return packet;
            }

            public bool ReadPacket(out ClientMediaPacket packet)
            {
                var result = _packetChannel.Reader.TryRead(out packet);

                if (result)
                {
                    Interlocked.Add(ref _outstandingPacketsSize, -packet.RentedPayload.Size);
                    Interlocked.Decrement(ref _outstandingPacketCount);
                }

                return result;
            }

            public void Complete()
            {
                _tcs.TrySetResult();
            }

            public Task UntilCompleteAsync()
            {
                return _tcs.Task;
            }

            private bool ShouldSkipPacket(ClientMediaContext context, bool isSkippable)
            {
                return _mediaPacketDiscarder.ShouldDiscardPacket(
                    isSkippable, context.OutstandingPacketsSize, context.OutstandingPacketsCount);
            }
        }

        private record struct ClientMediaPacket(IRtmpSubscribeStreamContext StreamContext, IRentedBuffer RentedPayload, bool IsSkippable, uint Timestamp, MediaType MediaType);
    }
}
