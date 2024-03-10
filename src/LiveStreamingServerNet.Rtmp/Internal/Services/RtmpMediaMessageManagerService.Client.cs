using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Logging;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal partial class RtmpMediaMessageManagerService
    {
        private readonly ConcurrentDictionary<IRtmpClientContext, ClientMediaContext> _clientMediaContexts = new();
        private readonly ConcurrentDictionary<IRtmpClientContext, Task> _clientTasks = new();

        private ClientMediaContext? GetMediaContext(IRtmpClientContext clientContext)
        {
            return _clientMediaContexts.GetValueOrDefault(clientContext);
        }

        public void RegisterClient(IRtmpClientContext clientContext)
        {
            _clientMediaContexts[clientContext] = new ClientMediaContext(clientContext, _config, _logger);

            var clientTask = Task.Run(() => ClientTask(clientContext));
            _clientTasks[clientContext] = clientTask;
            _ = clientTask.ContinueWith(_ => _clientTasks.TryRemove(clientContext, out var _));
        }

        public void UnregisterClient(IRtmpClientContext clientContext)
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

        private async Task ClientTask(IRtmpClientContext clientContext)
        {
            var context = _clientMediaContexts[clientContext];
            var cancellation = context.CancellationToken;

            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var package = await context.ReadPackageAsync(cancellation);

                    try
                    {
                        if (clientContext.StreamSubscriptionContext == null)
                            continue;

                        await clientContext.StreamSubscriptionContext.UntilInitializationComplete();

                        await SendMediaPackageAsync(
                            clientContext,
                            package.MediaType,
                            package.RentedPayload.Buffer,
                            package.RentedPayload.Size,
                            package.Timestamp,
                            package.StreamId,
                            cancellation);
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
                    catch (Exception ex)
                    {
                        _logger.FailedToSendMediaMessage(clientContext.Client.ClientId, ex);
                    }
                    finally
                    {
                        package.RentedPayload.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }

            while (context.ReadPackage(out var package))
            {
                package.RentedPayload.Unclaim();
            }
        }

        private class ClientMediaContext
        {
            public readonly IRtmpClientContext ClientContext;
            public readonly CancellationToken CancellationToken;
            public long OutstandingPackagesSize => _outstandingPackagesSize;
            public long OutstandingPackagesCount => _packageChannel.Reader.Count;

            private readonly MediaMessageConfiguration _config;
            private readonly ILogger _logger;

            private readonly Channel<ClientMediaPackage> _packageChannel;
            private readonly CancellationTokenSource _cts;

            private long _outstandingPackagesSize;
            private bool _skippingPackage;

            public ClientMediaContext(IRtmpClientContext clientContext, MediaMessageConfiguration config, ILogger logger)
            {
                ClientContext = clientContext;
                _config = config;
                _logger = logger;

                _packageChannel = Channel.CreateUnbounded<ClientMediaPackage>();
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
                if (ShouldSkipPackage(this, ref package))
                {
                    return false;
                }

                if (!_packageChannel.Writer.TryWrite(package))
                {
                    return false;
                }

                Interlocked.Add(ref _outstandingPackagesSize, package.RentedPayload.Size);
                return true;
            }

            public async ValueTask<ClientMediaPackage> ReadPackageAsync(CancellationToken cancellation)
            {
                var package = await _packageChannel.Reader.ReadAsync(cancellation);
                Interlocked.Add(ref _outstandingPackagesSize, -package.RentedPayload.Size);
                return package;
            }

            public bool ReadPackage(out ClientMediaPackage package)
            {
                return _packageChannel.Reader.TryRead(out package);
            }

            private bool ShouldSkipPackage(ClientMediaContext context, ref ClientMediaPackage package)
            {
                if (!package.IsSkippable)
                {
                    _skippingPackage = false;
                    return false;
                }

                if (_skippingPackage)
                {
                    if (context.OutstandingPackagesSize <= _config.TargetOutstandingMediaMessageSize &&
                        context.OutstandingPackagesCount <= _config.TargetOutstandingMediaMessageCount)
                    {
                        _logger.ResumeMediaPackage(ClientContext.Client.ClientId, context.OutstandingPackagesSize, context.OutstandingPackagesCount);
                        _skippingPackage = false;
                        return false;
                    }

                    return true;
                }

                if (context.OutstandingPackagesSize > _config.MaxOutstandingMediaMessageSize ||
                    context.OutstandingPackagesCount > _config.MaxOutstandingMediaMessageCount)
                {
                    _logger.PauseMediaPackage(ClientContext.Client.ClientId, context.OutstandingPackagesSize, context.OutstandingPackagesCount);
                    _skippingPackage = true;
                    return true;
                }

                return false;
            }
        }

        private record struct ClientMediaPackage(
            MediaType MediaType,
            uint Timestamp,
            uint StreamId,
            IRentedBuffer RentedPayload,
            bool IsSkippable);
    }
}
