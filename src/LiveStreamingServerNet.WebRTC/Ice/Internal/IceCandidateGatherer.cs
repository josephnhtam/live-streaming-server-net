using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Ice.Configurations;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes;
using LiveStreamingServerNet.WebRTC.Utilities;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal class IceCandidateGatherer : IIceCandidateGatherer
    {
        private readonly IUdpTransportFactory _transportFactory;
        private readonly IStunAgentFactory _stunAgentFactory;
        private readonly IStunDnsResolver _stunDnsResolver;
        private readonly IceGathererConfiguration _config;

        private readonly ResiliencePipeline _bindingPipeline;

        private volatile int _isGathering;
        private CancellationTokenSource? _cts;
        private Task? _gatheringTask;

        public event EventHandler<LocalIceCandidate?>? OnGathered;

        public IceCandidateGatherer(
            IUdpTransportFactory transportFactory,
            IStunAgentFactory stunAgentFactory,
            IStunDnsResolver stunDnsResolver,
            IceGathererConfiguration config)
        {
            _transportFactory = transportFactory;
            _stunAgentFactory = stunAgentFactory;
            _stunDnsResolver = stunDnsResolver;
            _config = config;

            _bindingPipeline = CreateStunBindingPipeline(_config);
        }

        private async Task GatherCandidatesAsync(
            IceCandidateGatheringContext context,
            CancellationToken cancellation)
        {
            try
            {
                var localAddresses = NetworkUtility.GetLocalIPAddresses();

                var hostCandidates = GatherHostCandidates(context, localAddresses);

                if (!hostCandidates.Any())
                {
                    NotifyEndOfGathering();
                    return;
                }

                await ResolveStunServerEndPointsAsync(context, cancellation).ConfigureAwait(false);

                await GatherServerReflexiveCandidatesAsync(context, hostCandidates, cancellation).ConfigureAwait(false);

                NotifyEndOfGathering();
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            catch (Exception)
            {
                // todo: add logs
            }
        }

        private List<LocalIceCandidate> GatherHostCandidates(
            IceCandidateGatheringContext context,
            IEnumerable<IPAddress> localAddresses)
        {
            var hostCandidates = new List<LocalIceCandidate>();

            foreach (var address in localAddresses)
            {
                var candidate = TryCreateHostCandidate(address);
                if (candidate == null)
                    continue;

                hostCandidates.Add(candidate);
                NotifyCandidateGathered(candidate);
            }

            return hostCandidates;

            LocalIceCandidate? TryCreateHostCandidate(IPAddress address)
            {
                try
                {
                    var socket = CreateBoundUdpSocket(address);

                    if (socket == null)
                        return null;

                    var endPoint = (socket.LocalEndPoint as IPEndPoint)!;

                    var iceEndPoint = CreateIceEndPoint(socket);
                    iceEndPoint.Start();

                    return new LocalIceCandidate
                    (
                        IceEndPoint: iceEndPoint,
                        BoundEndPoint: endPoint,
                        EndPoint: endPoint,
                        Type: IceCandidateType.Host,
                        Foundation: IceFoundation.Create(IceCandidateType.Host, address)
                    );
                }
                catch (Exception ex)
                {
                    // todo: add logs
                    return null;
                }
            }
        }

        private async ValueTask ResolveStunServerEndPointsAsync(
            IceCandidateGatheringContext context,
            CancellationToken cancellation)
        {
            var allEndPoints = new ConcurrentBag<IPEndPoint>();

            try
            {
                await Parallel.ForEachAsync(
                    _config.StunServers,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = _config.StunDnsResolutionMaxConcurrency,
                        CancellationToken = cancellation
                    },
                    ResolveDnsAsync
                ).ConfigureAwait(false);

                var allEndPointsList = allEndPoints.ToList();
                context.SetStunServerEndPoints(allEndPointsList, AddressFamily.InterNetwork);
                context.SetStunServerEndPoints(allEndPointsList, AddressFamily.InterNetworkV6);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                // todo: add logs
            }

            return;

            async ValueTask ResolveDnsAsync(string stunServer, CancellationToken token)
            {
                try
                {
                    var endPoints = await _stunDnsResolver.ResolveAsync(stunServer, token).ConfigureAwait(false);

                    foreach (var endPoint in endPoints)
                    {
                        allEndPoints.Add(endPoint);
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception)
                {
                    // todo: add logs
                }
            }
        }

        private async Task GatherServerReflexiveCandidatesAsync(
            IceCandidateGatheringContext context,
            IReadOnlyList<LocalIceCandidate> hostCandidates,
            CancellationToken cancellation)
        {
            try
            {
                await Parallel.ForEachAsync(
                    hostCandidates,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = _config.StunBindingMaxConcurrency,
                        CancellationToken = cancellation
                    },
                    TryCreateServerReflexiveCandidateAsync
                ).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                // todo: add logs
            }

            return;

            async ValueTask TryCreateServerReflexiveCandidateAsync(LocalIceCandidate hostCandidate, CancellationToken token)
            {
                try
                {
                    await _bindingPipeline.ExecuteAsync(async ct =>
                    {
                        var addressFamily = hostCandidate.BoundEndPoint.AddressFamily;
                        var stunServerEndPoint = context.GetStunServerEndPoint(addressFamily);

                        if (stunServerEndPoint == null)
                            return;

                        using var bindingRequest = CreateBindingRequest();

                        using var result = await hostCandidate.IceEndPoint
                            .SendStunRequestAsync(bindingRequest, stunServerEndPoint, ct)
                            .ConfigureAwait(false);

                        var bindingResponse = result.Message;
                        var mappedEndPoint = GetMappedEndPoint(bindingResponse);

                        if (mappedEndPoint == null)
                            return;

                        var serverReflexiveCandidate = new LocalIceCandidate(
                            IceEndPoint: hostCandidate.IceEndPoint,
                            BoundEndPoint: hostCandidate.BoundEndPoint,
                            EndPoint: mappedEndPoint,
                            Type: IceCandidateType.ServerReflexive,
                            Foundation: IceFoundation.Create(
                                IceCandidateType.ServerReflexive, hostCandidate.BoundEndPoint.Address)
                        );

                        NotifyCandidateGathered(serverReflexiveCandidate);
                    }, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception)
                {
                    // todo: add logs
                }
            }
        }

        private static IPEndPoint? GetMappedEndPoint(StunMessage bindingResponse)
        {
            var xorMappedAddressAttr = bindingResponse.Attributes
                .OfType<XorMappedAddressAttribute>().FirstOrDefault();

            if (xorMappedAddressAttr != null)
            {
                return xorMappedAddressAttr.EndPoint;
            }

            var mappedAddressAttr = bindingResponse.Attributes
                .OfType<MappedAddressAttribute>().FirstOrDefault();

            if (mappedAddressAttr != null)
            {
                return mappedAddressAttr.EndPoint;
            }

            return null;
        }

        private StunMessage CreateBindingRequest()
        {
            return new StunMessage(StunClass.Request, StunMethods.BindingRequest, []);
        }

        private Socket? CreateBoundUdpSocket(IPAddress address)
        {
            try
            {
                return NetworkUtility.CreateBoundUdpSocket(address);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private IIceEndPoint CreateIceEndPoint(Socket socket)
        {
            return new IceEndPoint(socket, _transportFactory, _stunAgentFactory);
        }

        private void NotifyCandidateGathered(LocalIceCandidate candidate)
        {
            try
            {
                OnGathered?.Invoke(this, candidate);
            }
            catch (Exception)
            {
                // todo: add logs
            }
        }

        private void NotifyEndOfGathering()
        {
            try
            {
                OnGathered?.Invoke(this, null);
            }
            catch (Exception)
            {
                // todo: add logs
            }
        }

        public bool StartGathering()
        {
            if (Interlocked.Exchange(ref _isGathering, 1) != 0)
                return false;

            var context = new IceCandidateGatheringContext();

            _cts = new CancellationTokenSource();
            _gatheringTask = Task.Run(() => GatherCandidatesAsync(context, _cts.Token));
            return true;
        }

        public async ValueTask<bool> StopGatheringAsync()
        {
            if (_isGathering != 1)
                return false;

            Debug.Assert(_cts != null);
            Debug.Assert(_gatheringTask != null);

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            await ErrorBoundary.ExecuteAsync(async () => await _gatheringTask.ConfigureAwait(false))
                .ConfigureAwait(false);

            _gatheringTask = null;

            _isGathering = 0;
            return true;
        }

        private static ResiliencePipeline CreateStunBindingPipeline(IceGathererConfiguration config)
        {
            var builder = new ResiliencePipelineBuilder();

            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = config.StunBindingMaxRetries,
                Delay = config.StunBindingRetryBaseDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,

                ShouldHandle = new PredicateBuilder()
                    .Handle<SocketException>()
                    .Handle<TimeoutException>()
                    .Handle<TimeoutRejectedException>()
            });

            if (config.StunBindingAttemptTimeout > TimeSpan.Zero)
            {
                builder.AddTimeout(config.StunBindingAttemptTimeout);
            }

            return builder.Build();
        }

        private class IceCandidateGatheringContext
        {
            private readonly ConcurrentDictionary<AddressFamily, IPEndPoint[]> _stunEndPointsByFamily = new();
            private readonly ConcurrentDictionary<AddressFamily, uint> _stunIndexByFamily = new();

            public void SetStunServerEndPoints(List<IPEndPoint> endPoints, AddressFamily addressFamily)
            {
                var list = endPoints.Where(ep => ep.AddressFamily == addressFamily)
                    .DistinctBy(ep => (ep.Address, ep.Port))
                    .ToArray();

                if (!list.Any())
                    return;

                _stunEndPointsByFamily[addressFamily] = list;
            }

            public IPEndPoint? GetStunServerEndPoint(AddressFamily addressFamily)
            {
                if (!_stunEndPointsByFamily.TryGetValue(addressFamily, out var addresses) || !addresses.Any())
                    return null;

                var idx = _stunIndexByFamily.AddOrUpdate(addressFamily, 0, static (_, prev) => prev + 1);
                return addresses[idx % addresses.Length];
            }
        }
    }
}
