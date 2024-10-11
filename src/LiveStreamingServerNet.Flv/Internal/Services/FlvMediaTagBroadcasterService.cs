using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Logging;
using LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarding.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvMediaTagBroadcasterService : IFlvMediaTagBroadcasterService, IAsyncDisposable
    {
        private readonly IMediaPacketDiscarderFactory _mediaPacketDiscarderFactory;
        private readonly IFlvMediaTagSenderService _mediaTagSender;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<IFlvClient, ClientMediaContext> _clientMediaContexts = new();
        private readonly ConcurrentDictionary<IFlvClient, Task> _clientTasks = new();

        public FlvMediaTagBroadcasterService(
            IMediaPacketDiscarderFactory mediaPacketDiscarderFactory,
            IFlvMediaTagSenderService mediaTagSender,
            ILogger<FlvMediaTagBroadcasterService> logger)
        {
            _mediaPacketDiscarderFactory = mediaPacketDiscarderFactory;
            _mediaTagSender = mediaTagSender;
            _logger = logger;
        }

        public ValueTask BroadcastMediaTagAsync(IFlvStreamContext streamContext, IReadOnlyList<IFlvClient> subscribers, MediaType mediaType, uint timestamp, bool isSkippable, IRentedBuffer rentedBuffer)
        {
            if (!subscribers.Any())
                return ValueTask.CompletedTask;

            rentedBuffer.Claim(subscribers.Count);

            var mediaPacket = new ClientMediaPacket(
                mediaType,
                timestamp,
                rentedBuffer,
                isSkippable);

            foreach (var subscriber in subscribers)
            {
                var mediaContext = GetMediaContext(subscriber);
                if (mediaContext == null || !mediaContext.AddPacket(ref mediaPacket))
                    rentedBuffer.Unclaim();
            }

            return ValueTask.CompletedTask;
        }

        private ClientMediaContext? GetMediaContext(IFlvClient clientContext)
        {
            return _clientMediaContexts.GetValueOrDefault(clientContext);
        }

        public void RegisterClient(IFlvClient client)
        {
            var mediaPacketDiscarder = _mediaPacketDiscarderFactory.Create(client.ClientId);
            var context = new ClientMediaContext(client, mediaPacketDiscarder);
            _clientMediaContexts[client] = context;

            var clientTask = Task.Run(() => ClientTask(context));
            _clientTasks[client] = clientTask;
            _ = clientTask.ContinueWith(_ => _clientTasks.TryRemove(client, out var _));
        }

        public void UnregisterClient(IFlvClient client)
        {
            if (_clientMediaContexts.TryRemove(client, out var context))
            {
                context.Stop();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_clientTasks.Values);
        }

        private async Task ClientTask(ClientMediaContext context)
        {
            var client = context.Client;
            var cancellation = context.CancellationToken;

            try
            {
                await client.UntilInitializationCompleteAsync();

                while (!cancellation.IsCancellationRequested)
                {
                    var packet = await context.ReadPacketAsync(cancellation);

                    try
                    {
                        await _mediaTagSender.SendMediaTagAsync(
                            client,
                            packet.MediaType,
                            packet.RentedPayload.Buffer,
                            packet.RentedPayload.Size,
                            packet.Timestamp,
                            cancellation);
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
                    catch (Exception ex)
                    {
                        _logger.FailedToSendMediaMessage(client.ClientId, ex);
                    }
                    finally
                    {
                        packet.RentedPayload.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            catch (ChannelClosedException) { }
            catch (Exception ex)
            {
                _logger.FailedToSendMediaMessage(client.ClientId, ex);
            }

            while (context.ReadPacket(out var packet))
            {
                packet.RentedPayload.Unclaim();
            }
        }

        private class ClientMediaContext
        {
            public readonly IFlvClient Client;
            public readonly CancellationToken CancellationToken;
            public long OutstandingPacketsSize => _outstandingPacketsSize;
            public long OutstandingPacketsCount => _outstandingPacketCount;

            private readonly IMediaPacketDiscarder _mediaPacketDiscarder;

            private readonly Channel<ClientMediaPacket> _packetChannel;
            private readonly CancellationTokenSource _cts;

            private long _outstandingPacketsSize;
            private long _outstandingPacketCount;

            public ClientMediaContext(IFlvClient client, IMediaPacketDiscarder mediaPacketDiscarder)
            {
                Client = client;
                _mediaPacketDiscarder = mediaPacketDiscarder;

                _packetChannel = Channel.CreateUnbounded<ClientMediaPacket>(
                    new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = true });
                _cts = new CancellationTokenSource();
                CancellationToken = _cts.Token;
            }

            public void Stop()
            {
                _packetChannel.Writer.Complete();
                _cts.Cancel();
            }

            public bool AddPacket(ref ClientMediaPacket packet)
            {
                if (ShouldSkipPacket(this, ref packet))
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

            private bool ShouldSkipPacket(ClientMediaContext context, ref ClientMediaPacket packet)
            {
                return _mediaPacketDiscarder.ShouldDiscardMediaPacket(
                    packet.IsSkippable, context.OutstandingPacketsSize, context.OutstandingPacketsCount);
            }
        }

        private record struct ClientMediaPacket(
            MediaType MediaType,
            uint Timestamp,
            IRentedBuffer RentedPayload,
            bool IsSkippable);
    }
}
