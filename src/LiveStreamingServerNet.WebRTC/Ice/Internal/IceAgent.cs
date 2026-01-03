using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Ice.Configurations;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Ice.Internal.StunAttributes;
using LiveStreamingServerNet.WebRTC.Stun.Internal;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using System.Collections.Concurrent;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal partial class IceAgent : IIceAgent
    {
        private readonly IceCredentials _credentials;
        private readonly IceAgentConfiguration _config;

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

        public IceRole Role { get; private set; }
        public IceConnectionState ConnectionState { get; private set; }
        public event EventHandler<IceConnectionState>? OnStateChanged;
        public event EventHandler<IceCandidate?>? OnLocalCandidateGathered;

        public IceAgent(
            IceRole role,
            IceCredentials credentials,
            IceAgentConfiguration config,
            IIceCandidateGathererFactory candidateGathererFactory,
            ulong? tieBreaker = null)
        {
            Role = role;
            _credentials = credentials;
            _config = config;

            _tieBreaker = tieBreaker ?? RandomNumberUtility.GetRandomUInt64();
            ConnectionState = IceConnectionState.New;

            _candidateGatherer = candidateGathererFactory.Create();
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

        private bool CheckCompletion()
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

                CheckCompletion();
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
                _checkList.TriggerCheck(toNominate);
            }
        }

        private bool IsConnectivityCheckMaxConcurrencyReached()
        {
            lock (_syncLock)
            {
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
            IceRole requestRole;
            bool sentUseCandidate;

            lock (_syncLock)
            {
                requestRole = Role;

                sentUseCandidate =
                    Role == IceRole.Controlling &&
                    reason == ConnectivityCheckReason.Check &&
                    pair.NominationState == IceCandidateNominationState.ControllingNominating;
            }

            var attributes = CreateStunAttributes(requestRole, sentUseCandidate);

            using var request = new StunMessage(
                    StunClass.Request,
                    StunMethods.BindingRequest,
                    attributes)
                .WithMessageIntegrity(_credentials.PwdRemoteBytes)
                .WithFingerprint();

            try
            {
                using var result = await pair.SendStunRequestAsync(request, cancellation).ConfigureAwait(false);

                var response = result.Message;
                var remoteEndPoint = result.RemoteEndPoint;

                lock (_syncLock)
                {
                    if (TryHandleRoleConflictError(requestRole, response))
                    {
                        ScheduleConnectivityCheck(pair, reason);
                        return;
                    }

                    if (response is not { Class: StunClass.SuccessResponse, Method: StunMethods.BindingRequest })
                    {
                        OnCheckFailed(pair, reason, sentUseCandidate);
                        return;
                    }

                    TryAdoptPeerReflexiveCandidate(pair.LocalCandidate.IceEndPoint, remoteEndPoint);

                    OnCheckSucceeded(pair, reason, sentUseCandidate);
                }
            }
            catch (Exception)
            {
                OnCheckFailed(pair, reason, sentUseCandidate);
            }

            return;

            List<IStunAttribute> CreateStunAttributes(IceRole requestRole, bool isControllingNominating)
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

            bool TryHandleRoleConflictError(IceRole requestRole, StunMessage response)
            {
                if (response.Class != StunClass.ErrorResponse)
                    return false;

                var errorCodeAttr = response.Attributes.OfType<ErrorCodeAttribute>().FirstOrDefault();
                if (errorCodeAttr?.Code != 487)
                    return false;

                var newRole = (requestRole == IceRole.Controlling) ? IceRole.Controlled : IceRole.Controlling;
                SwitchRole(newRole);
                return true;
            }
        }

        private void TryAdoptPeerReflexiveCandidate(IIceEndPoint endPoint, IPEndPoint remoteEndPoint)
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

                _checkList.AddRemoteCandidate(prflxCandidate, isTriggered: true);
            }
        }

        private void OnCheckSucceeded(IceCandidatePair pair, ConnectivityCheckReason reason, bool sentUseCandidate)
        {
            lock (_syncLock)
            {
                pair.State = IceCandidatePairState.Succeeded;
                _validPairs.Add(pair);

                if (reason == ConnectivityCheckReason.Check)
                {
                    if (Role == IceRole.Controlling)
                    {
                        OnControllingCheckSucceeded();
                    }
                    else
                    {
                        OnControlledCheckSucceeded();
                    }
                }

                _checkList.UnfreezePairsWithFoundation(pair.Foundation);
                CheckCompletion();
            }

            return;

            void OnControllingCheckSucceeded()
            {
                if (pair.NominationState == IceCandidateNominationState.ControlledNominating)
                {
                    pair.NominationState = IceCandidateNominationState.None;
                    return;
                }

                if (pair.NominationState == IceCandidateNominationState.ControllingNominating)
                {
                    if (sentUseCandidate)
                    {
                        SelectPair(pair);
                    }
                    else
                    {
                        _checkList.TriggerCheck(pair);
                    }
                }
            }

            void OnControlledCheckSucceeded()
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

        private void OnCheckFailed(IceCandidatePair pair, ConnectivityCheckReason reason, bool sentUseCandidate)
        {
            lock (_syncLock)
            {
                pair.State = IceCandidatePairState.Failed;

                if (sentUseCandidate && pair.NominationState is IceCandidateNominationState.ControllingNominating)
                {
                    pair.NominationState = IceCandidateNominationState.None;
                }

                if (_selectedPair == pair)
                {
                    _selectedPair = null;
                    TryTransitionTo(IceConnectionState.Disconnected, expected: IceConnectionStateFlag.Connected | IceConnectionStateFlag.Completed);
                }

                _validPairs.Remove(pair);

                CheckCompletion();
            }
        }

        private void LocalCandidateGathered(object gatherer, LocalIceCandidate? candidate)
        {
            lock (_syncLock)
            {
                if (ConnectionState is (IceConnectionState.Completed or IceConnectionState.Failed or IceConnectionState.Closed))
                    return;

                OnLocalCandidateGathered?.Invoke(this, candidate);

                if (candidate == null)
                {
                    _localGatheringComplete = true;
                    CheckCompletion();
                    return;
                }

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
                TryAdoptPeerReflexiveCandidate(endPoint, remoteEndPoint);

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
                    if (iceControlling.TieBreaker >= _tieBreaker)
                    {
                        SwitchRole(IceRole.Controlled);
                        return null;
                    }

                    return BindingResult.RoleConflict;
                }

                if (Role == IceRole.Controlled && iceControlled != null)
                {
                    return BindingResult.RoleConflict;
                }

                return null;
            }

            void TryHandlePriorityUpdate(StunMessage request, IceCandidatePair pair)
            {
                var priorityAttr = request.Attributes.OfType<PriorityAttribute>().FirstOrDefault();

                if (priorityAttr == null)
                    return;

                pair.RemoteCandidate.Priority = priorityAttr.Priority;
                pair.RefreshPriority(Role == IceRole.Controlling);
            }

            void TryHandleUseCandidate(StunMessage request, IceCandidatePair pair)
            {
                if (!request.Attributes.OfType<UseCandidateAttribute>().Any())
                    return;

                pair.UseCandidateReceived = true;

                if (Role != IceRole.Controlled)

                    return;

                pair.NominationState = IceCandidateNominationState.ControlledNominating;
                _checkList.TriggerCheck(pair);
            }

            void TriggerConnectivityCheck(IceCandidatePair pair)
            {
                if (pair.State == IceCandidatePairState.Succeeded)
                    return;

                _checkList.TriggerCheck(pair);
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
                    CheckCompletion();
                    return;
                }

                _checkList.AddRemoteCandidate(candidate, isTriggered: false);
            }
        }

        private void SwitchRole(IceRole role)
        {
            lock (_syncLock)
            {
                if (Role == role)
                    return;

                Role = role;
                _checkList.UpdatePairsForRoleSwitch();
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
