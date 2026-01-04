using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Ice.Configurations;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Logging;
using LiveStreamingServerNet.WebRTC.Ice.Internal.StunAttributes;
using LiveStreamingServerNet.WebRTC.Stun.Internal;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal partial class IceAgent : IIceAgent
    {
        private readonly IceCredentials _credentials;
        private readonly IceAgentConfiguration _config;
        private readonly ILogger _logger;

        private readonly ulong _tieBreaker;
        private readonly object _syncLock = new object();

        private readonly IIceCandidateGatherer _candidateGatherer;
        private readonly CheckList _checkList;
        private readonly StunMessageHandler _stunMessageHandler;

        private readonly CancellationTokenSource _cts;
        private readonly HashSet<IceCandidatePair> _validPairs;
        private readonly ConcurrentDictionary<Task, object?> _connectivityCheckTasks;

        private bool _localGatheringComplete;
        private bool _remoteGatheringComplete;
        private IceCandidatePair? _selectedPair;

        private Task? _checkerTask;
        private Task? _keepaliveTask;
        private Task? _nominatingTask;

        public string Identifier { get; private set; }
        public IceRole Role { get; private set; }
        public IceConnectionState ConnectionState { get; private set; }
        public event EventHandler<IceConnectionState>? OnStateChanged;
        public event EventHandler<IceCandidate?>? OnLocalCandidateGathered;

        public IceAgent(
            string identifier,
            IceRole role,
            IceCredentials credentials,
            IceAgentConfiguration config,
            IIceCandidateGathererFactory candidateGathererFactory,
            ILogger<IceAgent> logger,
            ulong? tieBreaker = null)
        {
            Identifier = identifier;
            Role = role;

            _credentials = credentials;
            _config = config;
            _logger = logger;

            _tieBreaker = tieBreaker ?? RandomNumberUtility.GetRandomUInt64();
            ConnectionState = IceConnectionState.New;

            _candidateGatherer = candidateGathererFactory.Create(identifier);
            _candidateGatherer.OnGathered += LocalCandidateGathered;

            _checkList = new CheckList(this);
            _stunMessageHandler = new StunMessageHandler(this);

            _cts = new CancellationTokenSource();
            _validPairs = new HashSet<IceCandidatePair>();
            _connectivityCheckTasks = new ConcurrentDictionary<Task, object?>();
        }

        public bool Start()
        {
            if (!TryTransitionTo(IceConnectionState.Checking, expected: IceConnectionStateFlag.New))
                return false;

            _logger.IceAgentStarted(Identifier, Role);

            StartGathering();
            _checkerTask = Task.Run(() => CheckerLoopAsync(_cts.Token));
            _keepaliveTask = Task.Run(() => KeepaliveLoopAsync(_cts.Token));
            return true;
        }

        public async ValueTask<bool> StopAsync()
        {
            if (!TryTransitionTo(IceConnectionState.Closed))
            {
                return false;
            }

            _logger.IceAgentStopped(Identifier, Role);

            _cts.Cancel();
            await StopGatheringAsync().ConfigureAwait(false);
            return true;
        }

        private async ValueTask<bool> FailAsync()
        {
            if (!TryTransitionTo(IceConnectionState.Failed, excluded: IceConnectionStateFlag.Closed))
            {
                return false;
            }

            _cts.Cancel();
            await StopGatheringAsync().ConfigureAwait(false);
            return true;
        }

        private void StartGathering()
        {
            _candidateGatherer.StartGathering();
        }

        private async ValueTask StopGatheringAsync()
        {
            await ErrorBoundary.ExecuteAsync(async () =>
                await _candidateGatherer.StopGatheringAsync().ConfigureAwait(false)
            ).ConfigureAwait(false);
        }

        private async Task CheckerLoopAsync(CancellationToken cancellation)
        {
            var checkInterval = TimeSpanUtility.Max(TimeSpan.FromMilliseconds(50), _config.ConnectivityCheckInterval);
            await RunActiveAgentLoopAsync(checkInterval, CheckNextPair, cancellation).ConfigureAwait(false);

            return;

            void CheckNextPair()
            {
                lock (_syncLock)
                {
                    if (!IsConnectivityCheckMaxConcurrencyReached())
                    {
                        var pairToCheck = _checkList.GetNextPair();

                        if (pairToCheck != null)
                        {
                            ScheduleConnectivityCheck(pairToCheck, ConnectivityCheckReason.Check);
                            return;
                        }
                    }

                    if (CheckCompletion())
                    {
                        return;
                    }

                    TryNominateValidPair();
                }
            }

            bool CheckCompletion()
            {
                lock (_syncLock)
                {
                    if (IsFailed())
                    {
                        TryTransitionTo(IceConnectionState.Failed, expected:
                            IceConnectionStateFlag.Checking |
                            IceConnectionStateFlag.Connected |
                            IceConnectionStateFlag.Disconnected);

                        return true;
                    }

                    if (IsCompleted())
                    {
                        TryTransitionTo(IceConnectionState.Completed, expected:
                            IceConnectionStateFlag.Checking |
                            IceConnectionStateFlag.Connected |
                            IceConnectionStateFlag.Disconnected);

                        return true;
                    }

                    return false;
                }

                bool IsCompleted() =>
                    _localGatheringComplete && _remoteGatheringComplete &&
                    _selectedPair != null && _checkList.AllPairsChecked();

                bool IsFailed() =>
                    _localGatheringComplete && _remoteGatheringComplete &&
                    _selectedPair == null && _checkList.AllPairsChecked() && !_validPairs.Any();
            }
        }

        private async Task KeepaliveLoopAsync(CancellationToken cancellation)
        {
            var keepAliveInterval = TimeSpanUtility.Max(TimeSpan.FromMilliseconds(1000), _config.KeepAliveInterval);
            await RunActiveAgentLoopAsync(keepAliveInterval, KeepValidPairsAlive, cancellation).ConfigureAwait(false);

            return;

            void KeepValidPairsAlive()
            {
                lock (_syncLock)
                {
                    if (_selectedPair != null)
                    {
                        ScheduleConnectivityCheck(_selectedPair, ConnectivityCheckReason.KeepAlive);
                    }
                }
            }
        }

        private async Task RunActiveAgentLoopAsync(TimeSpan interval, Action action, CancellationToken cancellation)
        {
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    await Task.Delay(interval, cancellation).ConfigureAwait(false);

                    lock (_syncLock)
                    {
                        if (ConnectionState is IceConnectionState.Failed or IceConnectionState.Closed)
                            return;

                        action();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            catch (Exception ex)
            {
                await FailAsync().ConfigureAwait(false);
            }
        }

        private bool ShouldNominatePair(IceCandidatePair pair)
        {
            var selectedPriority = _selectedPair?.Priority ?? 0UL;
            return pair.Priority > selectedPriority;
        }

        private void SelectPair(IceCandidatePair pair)
        {
            lock (_syncLock)
            {
                foreach (var validPair in _validPairs.Where(p =>
                    p != pair && p.NominationState == IceCandidateNominationState.Nominated))
                {
                    validPair.NominationState = IceCandidateNominationState.WasNominated;
                }

                pair.NominationState = IceCandidateNominationState.Nominated;
                _selectedPair = pair;

                TryTransitionTo(IceConnectionState.Connected, expected:
                    IceConnectionStateFlag.Checking |
                    IceConnectionStateFlag.Disconnected);

                _logger.PairSelected(Identifier, Role, pair.LocalCandidate.EndPoint, pair.RemoteCandidate.EndPoint);
            }
        }

        private void TryNominateValidPair()
        {
            lock (_syncLock)
            {
                if (Role != IceRole.Controlling)
                    return;

                if (ConnectionState is IceConnectionState.Completed or IceConnectionState.Failed or IceConnectionState.Closed)
                    return;

                if (_validPairs.Any(p => p.NominationState is IceCandidateNominationState.ControllingNominating))
                    return;

                var toNominate = _validPairs
                    .Where(p => p.NominationState is IceCandidateNominationState.None or IceCandidateNominationState.WasNominated)
                    .Where(ShouldNominatePair)
                    .OrderByDescending(p => p.Priority)
                    .FirstOrDefault();

                if (toNominate == null)
                    return;

                toNominate.NominationState = IceCandidateNominationState.ControllingNominating;
                _checkList.TriggerCheck(toNominate, "ControllingNomination");

                _logger.NominatingPair(Identifier, Role, toNominate.LocalCandidate.EndPoint, toNominate.RemoteCandidate.EndPoint);
            }
        }

        private bool IsConnectivityCheckMaxConcurrencyReached()
        {
            lock (_syncLock)
            {
                if (_config.MaxConcurrentConnectivityChecks <= 0)
                    return false;

                return _connectivityCheckTasks.Count >= _config.MaxConcurrentConnectivityChecks;
            }
        }

        private void ScheduleConnectivityCheck(IceCandidatePair pair, ConnectivityCheckReason reason)
        {
            lock (_syncLock)
            {
                if (pair.State == IceCandidatePairState.InProgress)
                    return;

                pair.State = IceCandidatePairState.InProgress;

                var task = PerformConnectivityCheckAsync(pair, reason, _cts.Token);

                _connectivityCheckTasks.TryAdd(task, null);

                task.ContinueWith(t =>
                {
                    _connectivityCheckTasks.TryRemove(t, out _);
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        private async Task PerformConnectivityCheckAsync(IceCandidatePair pair, ConnectivityCheckReason reason, CancellationToken cancellation)
        {
            var (requestRole, isControllingNominating) = GetRequestRoleAndNominatingState();

            var attributes = CreateStunAttributes(pair, requestRole, isControllingNominating);

            using var request = new StunMessage(
                    StunClass.Request,
                    StunMethods.BindingRequest,
                    attributes)
                .WithMessageIntegrity(_credentials.PwdRemoteBytes)
                .WithFingerprint();

            _logger.SendingConnectivityCheck(
                Identifier, requestRole,
                pair.LocalCandidate.EndPoint,
                pair.RemoteCandidate.EndPoint,
                pair.Foundation,
                isControllingNominating);

            try
            {
                using var result = await pair.SendStunRequestAsync(request, cancellation).ConfigureAwait(false);

                var response = result.Message;
                var remoteEndPoint = result.RemoteEndPoint;

                lock (_syncLock)
                {
                    if (TryHandleRoleConflictError(response, requestRole))
                    {
                        OnCheckFailed(pair, isControllingNominating, "Role switched");
                        _checkList.TriggerCheck(pair, "RoleConflictRetry");
                        return;
                    }

                    if (response is not { Class: StunClass.SuccessResponse, Method: StunMethods.BindingRequest })
                    {
                        OnCheckFailed(pair, isControllingNominating, $"Invalid response: {response.Class}");
                        return;
                    }

                    TryAdoptPeerReflexiveCandidate(pair.LocalCandidate.IceEndPoint, remoteEndPoint, reason: "ConnectivityCheckSucceeded");

                    OnCheckSucceeded(pair, reason);
                }
            }
            catch (Exception ex)
            {
                OnCheckFailed(pair, isControllingNominating, ex.Message);
            }

            return;

            (IceRole requestRole, bool isControllingNominating) GetRequestRoleAndNominatingState()
            {
                lock (_syncLock)
                {
                    var controllingNominating =
                        Role == IceRole.Controlling &&
                        reason == ConnectivityCheckReason.Check &&
                        pair.NominationState == IceCandidateNominationState.ControllingNominating;

                    return (Role, controllingNominating);
                }
            }

            List<IStunAttribute> CreateStunAttributes(IceCandidatePair pair, IceRole requestRole, bool isControllingNominating)
            {
                var stunAttributes = new List<IStunAttribute>
                {
                    new UsernameAttribute(_credentials.RequesterUsername),
                    new PriorityAttribute(pair.LocalCandidate.Priority),
                    requestRole == IceRole.Controlling ? new IceControllingAttribute(_tieBreaker) : new IceControlledAttribute(_tieBreaker),
                };

                if (isControllingNominating)
                {
                    stunAttributes.Add(new UseCandidateAttribute());
                }

                return stunAttributes;
            }

            bool TryHandleRoleConflictError(StunMessage response, IceRole requestRole)
            {
                if (response.Class != StunClass.ErrorResponse)
                    return false;

                var errorCodeAttr = response.Attributes.OfType<ErrorCodeAttribute>().FirstOrDefault();
                if (errorCodeAttr?.Code != 487)
                    return false;

                _logger.RoleConflictErrorReceived(Identifier, Role);

                var newRole = (requestRole == IceRole.Controlling) ? IceRole.Controlled : IceRole.Controlling;
                SwitchRole(newRole);
                return true;
            }

            void OnCheckSucceeded(IceCandidatePair pair, ConnectivityCheckReason reason)
            {
                lock (_syncLock)
                {
                    _logger.ConnectivityCheckSucceeded(
                        Identifier, Role,
                        pair.LocalCandidate.EndPoint,
                        pair.RemoteCandidate.EndPoint,
                        pair.Foundation,
                        isControllingNominating
                    );

                    pair.State = IceCandidatePairState.Succeeded;
                    _validPairs.Add(pair);

                    if (reason == ConnectivityCheckReason.Check)
                    {
                        if (Role == IceRole.Controlling)
                        {
                            OnControllingCheckSucceeded(pair);
                        }
                        else
                        {
                            OnControlledCheckSucceeded(pair);
                        }
                    }

                    _checkList.UnfreezePairsWithFoundation(pair.Foundation);
                }

                return;

                void OnControllingCheckSucceeded(IceCandidatePair pair)
                {
                    if (pair.NominationState == IceCandidateNominationState.ControlledNominating)
                    {
                        pair.NominationState = IceCandidateNominationState.None;
                        return;
                    }

                    if (pair.NominationState == IceCandidateNominationState.ControllingNominating)
                    {
                        if (isControllingNominating)
                        {
                            SelectPair(pair);
                        }
                        else
                        {
                            _checkList.TriggerCheck(pair, "ControllingNominationRetry");
                        }
                    }
                }

                void OnControlledCheckSucceeded(IceCandidatePair pair)
                {
                    if (pair.NominationState == IceCandidateNominationState.ControllingNominating)
                    {
                        pair.NominationState = IceCandidateNominationState.None;
                        return;
                    }

                    if (pair.UseCandidateReceived)
                    {
                        SelectPair(pair);
                    }
                }
            }

            void OnCheckFailed(IceCandidatePair pair, bool isControllingNominating, string failureReason)
            {
                lock (_syncLock)
                {
                    _logger.ConnectivityCheckFailed(
                        Identifier, Role,
                        pair.LocalCandidate.EndPoint,
                        pair.RemoteCandidate.EndPoint,
                        pair.Foundation,
                        isControllingNominating,
                        failureReason
                    );

                    pair.State = IceCandidatePairState.Failed;

                    if (isControllingNominating && pair.NominationState == IceCandidateNominationState.ControllingNominating)
                    {
                        pair.NominationState = IceCandidateNominationState.None;
                    }

                    if (_selectedPair == pair)
                    {
                        _selectedPair = null;
                        TryTransitionTo(IceConnectionState.Disconnected, expected: IceConnectionStateFlag.Connected | IceConnectionStateFlag.Completed);
                    }

                    _validPairs.Remove(pair);
                }
            }
        }

        private void TryAdoptPeerReflexiveCandidate(IIceEndPoint endPoint, IPEndPoint remoteEndPoint, string reason)
        {
            lock (_syncLock)
            {
                if (ConnectionState is (IceConnectionState.Completed or IceConnectionState.Failed or IceConnectionState.Closed))
                    return;

                if (_checkList.HasRemoteCandidate(remoteEndPoint))
                    return;

                var prflxCandidate = new RemoteIceCandidate(
                    EndPoint: remoteEndPoint,
                    Type: IceCandidateType.PeerReflexive,
                    Foundation: IceFoundation.Create(IceCandidateType.PeerReflexive, remoteEndPoint.Address)
                );

                _logger.PeerReflexiveCandidateAdopted(Identifier, Role, remoteEndPoint, prflxCandidate.Foundation, reason);
                _checkList.AddRemoteCandidate(prflxCandidate, peerReflexiveEndPoint: endPoint);
            }
        }

        private void LocalCandidateGathered(object gatherer, LocalIceCandidate? candidate)
        {
            lock (_syncLock)
            {
                if (ConnectionState is (IceConnectionState.Completed or IceConnectionState.Failed or IceConnectionState.Closed))
                    return;

                if (candidate == null)
                {
                    _localGatheringComplete = true;

                    _logger.LocalGatheringComplete(Identifier, Role);
                    OnLocalCandidateGathered?.Invoke(this, null);
                    return;
                }

                _logger.LocalCandidateGathered(Identifier, Role, candidate.EndPoint, candidate.Type);
                OnLocalCandidateGathered?.Invoke(this, candidate);

                if (_checkList.AddLocalCandidate(candidate))
                {
                    candidate.IceEndPoint.SetStunMessageHandler(_stunMessageHandler);
                }
            }
        }

        private BindingResult HandleIncomingBindingRequest(
            StunMessage request, IIceEndPoint endPoint, IPEndPoint remoteEndPoint)
        {
            lock (_syncLock)
            {
                TryAdoptPeerReflexiveCandidate(endPoint, remoteEndPoint, reason: "IncomingBindingRequest");

                var roleConflictResult = TryHandleRoleConflict(request);
                if (roleConflictResult.HasValue)
                {
                    return roleConflictResult.Value;
                }

                var pair = _checkList.FindPair(endPoint, remoteEndPoint);
                if (pair == null)
                {
                    return BindingResult.Error;
                }

                TryHandlePriorityUpdate(request, pair);

                TryHandleUseCandidate(request, pair);

                TriggerConnectivityCheck(pair);

                return BindingResult.Success;
            }

            BindingResult? TryHandleRoleConflict(StunMessage request)
            {
                var iceControlling = request.Attributes.OfType<IceControllingAttribute>().FirstOrDefault();
                var iceControlled = request.Attributes.OfType<IceControlledAttribute>().FirstOrDefault();

                if (iceControlled == null && iceControlling == null)
                {
                    return BindingResult.Error;
                }

                if (Role == IceRole.Controlling && iceControlling != null)
                {
                    _logger.RoleConflictDetected(Identifier, Role, iceControlling.TieBreaker, _tieBreaker);

                    if (iceControlling.TieBreaker >= _tieBreaker)
                    {
                        SwitchRole(IceRole.Controlled);
                        return null;
                    }

                    return BindingResult.RoleConflict;
                }

                if (Role == IceRole.Controlled && iceControlled != null)
                {
                    _logger.RoleConflictDetected(Identifier, Role, iceControlled.TieBreaker, _tieBreaker);
                    return BindingResult.RoleConflict;
                }

                return null;
            }

            void TryHandlePriorityUpdate(StunMessage request, IceCandidatePair pair)
            {
                var priorityAttr = request.Attributes.OfType<PriorityAttribute>().FirstOrDefault();

                if (priorityAttr == null)
                    return;

                var oldPriority = pair.Priority;
                pair.RemoteCandidate.Priority = priorityAttr.Priority;
                pair.RefreshPriority(Role == IceRole.Controlling);

                if (oldPriority != pair.Priority)
                {
                    _logger.PriorityUpdated(
                        Identifier, Role,
                        pair.LocalCandidate.EndPoint,
                        pair.RemoteCandidate.EndPoint,
                        oldPriority,
                        pair.Priority);
                }
            }

            void TryHandleUseCandidate(StunMessage request, IceCandidatePair pair)
            {
                if (!request.Attributes.OfType<UseCandidateAttribute>().Any())
                    return;

                _logger.UseCandidateReceived(
                    Identifier, Role,
                    pair.LocalCandidate.EndPoint,
                    pair.RemoteCandidate.EndPoint);

                pair.UseCandidateReceived = true;

                if (Role != IceRole.Controlled)
                    return;

                pair.NominationState = IceCandidateNominationState.ControlledNominating;
                _checkList.TriggerCheck(pair, "ControlledNomination");
            }

            void TriggerConnectivityCheck(IceCandidatePair pair)
            {
                if (pair.State == IceCandidatePairState.Succeeded)
                    return;

                _checkList.TriggerCheck(pair, "BindingRequestReceived");
            }
        }

        public void AddRemoteCandidate(RemoteIceCandidate? candidate)
        {
            lock (_syncLock)
            {
                if (ConnectionState is (IceConnectionState.Completed or IceConnectionState.Failed or IceConnectionState.Closed))
                    return;

                if (candidate == null)
                {
                    _remoteGatheringComplete = true;
                    _logger.RemoteGatheringComplete(Identifier, Role);
                    return;
                }

                _logger.RemoteCandidateAdded(Identifier, Role, candidate.EndPoint, candidate.Type);
                _checkList.AddRemoteCandidate(candidate);
            }
        }

        private void SwitchRole(IceRole newRole)
        {
            lock (_syncLock)
            {
                if (Role == newRole)
                    return;

                var oldRole = Role;
                Role = newRole;
                _checkList.UpdatePairsForRoleSwitch();

                _logger.RoleSwitched(Identifier, oldRole, newRole);
            }
        }

        private bool TryTransitionTo(IceConnectionState newState, IceConnectionStateFlag? expected = null, IceConnectionStateFlag? excluded = null)
        {
            lock (_syncLock)
            {
                if (ConnectionState == newState)
                    return false;

                if (expected.HasValue && ((int)ConnectionState & (int)expected) == 0)
                    return false;

                if (excluded.HasValue && ((int)ConnectionState & (int)excluded) != 0)
                    return false;

                ConnectionState = newState;
                _logger.StateChanged(Identifier, Role, newState);
                OnStateChanged?.Invoke(this, newState);
                return true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await ErrorBoundary.ExecuteAsync(async () => await StopAsync().ConfigureAwait(false))
                .ConfigureAwait(false);

            _cts.Cancel();

            List<Task?> tasks = [_checkerTask, _keepaliveTask, _nominatingTask, .. _connectivityCheckTasks.Keys];
            await ErrorBoundary.ExecuteAsync(async () =>
                await Task.WhenAll(tasks.Where(t => t != null)!).ConfigureAwait(false)).ConfigureAwait(false);

            _cts.Dispose();
        }

        private enum ConnectivityCheckReason
        {
            Check,
            KeepAlive
        }

        private enum BindingResult
        {
            Success,
            RoleConflict,
            Error
        }
    }
}
