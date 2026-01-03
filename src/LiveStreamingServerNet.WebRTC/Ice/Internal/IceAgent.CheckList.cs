using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Logging;
using LiveStreamingServerNet.WebRTC.Utilities;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal partial class IceAgent
    {
        private class CheckList
        {
            private readonly IceAgent _agent;
            private readonly int _maxCheckListSize;
            private readonly object _syncLock;

            private readonly List<LocalIceCandidate> _localCandidates = new();
            private readonly List<RemoteIceCandidate> _remoteCandidates = new();
            private readonly List<IceCandidatePair> _pairs = new();
            private readonly Queue<IceCandidatePair> _triggeredChecks = new();

            public CheckList(IceAgent agent)
            {
                _agent = agent;
                _maxCheckListSize = agent._config.MaxCheckListSize;
                _syncLock = agent._syncLock;
            }

            public bool AddLocalCandidate(LocalIceCandidate localCandidate)
            {
                lock (_syncLock)
                {
                    if (_localCandidates.Any(c => c.EndPoint.IsEquivalent(localCandidate.EndPoint)))
                        return false;

                    var newPairs = new List<IceCandidatePair>();
                    var isControlling = _agent.Role == IceRole.Controlling;

                    _localCandidates.Add(localCandidate);
                    foreach (var remoteCandidate in _remoteCandidates)
                    {
                        if (!IceLogic.CanPairCandidates(localCandidate, remoteCandidate))
                            continue;

                        var pair = new IceCandidatePair(localCandidate, remoteCandidate, isControlling);

                        _pairs.Add(pair);
                        newPairs.Add(pair);
                        _agent.OnCandidatePairCreated(pair);
                    }

                    UnfreezeNewPairs(newPairs);
                    PrunePairs();
                    return true;
                }
            }

            public bool AddRemoteCandidate(RemoteIceCandidate remoteCandidate, IIceEndPoint? triggeredEndPoint = null)
            {
                lock (_syncLock)
                {
                    if (_remoteCandidates.Any(c => c.EndPoint.IsEquivalent(remoteCandidate.EndPoint)))
                        return false;

                    var isTriggered = false;
                    var isControlling = _agent.Role == IceRole.Controlling;
                    var newPairs = new List<IceCandidatePair>();

                    _remoteCandidates.Add(remoteCandidate);

                    foreach (var localCandidate in _localCandidates)
                    {
                        if (!IceLogic.CanPairCandidates(localCandidate, remoteCandidate))
                            continue;

                        var pair = new IceCandidatePair(localCandidate, remoteCandidate, isControlling);

                        _pairs.Add(pair);
                        newPairs.Add(pair);

                        _agent.OnCandidatePairCreated(pair);

                        if (localCandidate.IceEndPoint == triggeredEndPoint)
                        {
                            TriggerCheck(pair, "RemoteCandidateAdded");
                            isTriggered = true;
                        }
                    }

                    if (!isTriggered)
                    {
                        UnfreezeNewPairs(newPairs);
                    }

                    PrunePairs();
                    return true;
                }
            }

            private void PrunePairs()
            {
                lock (_syncLock)
                {
                    var toRemoveCount = _pairs.Count - _maxCheckListSize;

                    if (toRemoveCount <= 0)
                    {
                        return;
                    }

                    var removables = _pairs
                        .Where(p => !p.IsTriggered)
                        .Where(p => p.State is
                            IceCandidatePairState.Failed or
                            IceCandidatePairState.Frozen or
                            IceCandidatePairState.Waiting)
                        .OrderBy(p =>
                        {
                            return p.State switch
                            {
                                IceCandidatePairState.Failed => 0,
                                IceCandidatePairState.Frozen => 1,
                                _ => 2
                            };
                        })
                        .ThenBy(p => p.Priority)
                        .Take(toRemoveCount)
                        .ToList();

                    foreach (var toRemove in removables)
                    {
                        _pairs.Remove(toRemove);
                    }
                }
            }

            public IceCandidatePair? GetNextPair()
            {
                lock (_syncLock)
                {
                    var triggeredCount = _triggeredChecks.Count;
                    var attempts = triggeredCount;

                    while (attempts-- > 0 && _triggeredChecks.TryDequeue(out var pair))
                    {
                        if (pair.State is IceCandidatePairState.InProgress)
                        {
                            _triggeredChecks.Enqueue(pair);
                            continue;
                        }

                        if (pair.State != IceCandidatePairState.Succeeded)
                            pair.State = IceCandidatePairState.Waiting;

                        pair.IsTriggered = false;

                        _agent._logger.GetNextPair(
                            _agent.Identifier, _agent.Role,
                            pair.LocalCandidate.EndPoint,
                            pair.RemoteCandidate.EndPoint,
                            pair.State,
                            pair.NominationState,
                            isTriggered: true,
                            triggeredCount,
                            _pairs.Count
                        );

                        return pair;
                    }

                    UnfreezePairs();

                    var result = _pairs
                        .Where(p => p.State == IceCandidatePairState.Waiting)
                        .OrderByDescending(p => p.Priority)
                        .FirstOrDefault();

                    if (result != null)
                    {
                        _agent._logger.GetNextPair(
                            _agent.Identifier, _agent.Role,
                            result.LocalCandidate.EndPoint,
                            result.RemoteCandidate.EndPoint,
                            result.State,
                            result.NominationState,
                            isTriggered: false,
                            triggeredCount,
                            _pairs.Count
                        );
                    }

                    return result;
                }
            }

            private void UnfreezePairs()
            {
                lock (_syncLock)
                {
                    if (_pairs.Any(p => p.State is IceCandidatePairState.Waiting or IceCandidatePairState.InProgress))
                        return;

                    var frozenByFoundation = _pairs
                        .Where(p => p.State == IceCandidatePairState.Frozen)
                        .GroupBy(p => p.Foundation);

                    foreach (var group in frozenByFoundation)
                    {
                        var highestPriority = group.OrderByDescending(p => p.Priority).First();
                        highestPriority.State = IceCandidatePairState.Waiting;
                    }
                }
            }

            private void UnfreezeNewPairs(List<IceCandidatePair> newPairs)
            {
                lock (_syncLock)
                {
                    if (!newPairs.Any())
                        return;

                    var activeFoundations = _pairs
                        .Where(p => !newPairs.Contains(p))
                        .Where(p => p.State is
                            IceCandidatePairState.Waiting or
                            IceCandidatePairState.InProgress)
                        .Select(p => p.Foundation)
                        .ToHashSet();

                    var newPairsByFoundation = newPairs
                        .Where(p => p.State == IceCandidatePairState.Frozen)
                        .GroupBy(p => p.Foundation);

                    foreach (var group in newPairsByFoundation)
                    {
                        var foundation = group.Key;

                        if (activeFoundations.Contains(foundation))
                        {
                            foreach (var pair in group)
                            {
                                pair.State = IceCandidatePairState.Waiting;
                            }
                        }
                        else
                        {
                            var highestPriority = group.OrderByDescending(p => p.Priority).First();
                            highestPriority.State = IceCandidatePairState.Waiting;
                        }
                    }
                }
            }

            public void UnfreezePairsWithFoundation(string foundation)
            {
                lock (_syncLock)
                {
                    foreach (var pair in _pairs)
                    {
                        if (pair.State == IceCandidatePairState.Frozen && pair.Foundation == foundation)
                        {
                            pair.State = IceCandidatePairState.Waiting;
                        }
                    }
                }
            }

            public void TriggerCheck(IceCandidatePair pair, string reason)
            {
                lock (_syncLock)
                {
                    if (pair.IsTriggered || pair.State == IceCandidatePairState.InProgress)
                        return;

                    if (pair.State != IceCandidatePairState.Succeeded)
                        pair.State = IceCandidatePairState.Waiting;

                    pair.IsTriggered = true;
                    _triggeredChecks.Enqueue(pair);

                    _agent._logger.TriggerCheck(
                        _agent.Identifier, _agent.Role,
                        pair.LocalCandidate.EndPoint,
                        pair.RemoteCandidate.EndPoint,
                        pair.State,
                        pair.NominationState,
                        _triggeredChecks.Count,
                        reason
                    );
                }
            }

            public bool AllPairsChecked()
            {
                lock (_syncLock)
                {
                    return _pairs.All(p => p is { IsTriggered: false, State: IceCandidatePairState.Succeeded or IceCandidatePairState.Failed });
                }
            }

            public bool HasRemoteCandidate(IPEndPoint remoteEndPoint)
            {
                lock (_syncLock)
                {
                    return _pairs.Any(p => p.RemoteCandidate.EndPoint.IsEquivalent(remoteEndPoint));
                }
            }

            public void UpdatePairsForRoleSwitch()
            {
                lock (_syncLock)
                {
                    var isControlling = _agent.Role == IceRole.Controlling;

                    foreach (var pair in _pairs)
                    {
                        if (pair.NominationState is
                            IceCandidateNominationState.ControllingNominating or
                            IceCandidateNominationState.ControlledNominating)
                        {
                            pair.NominationState = IceCandidateNominationState.None;
                        }

                        pair.RefreshPriority(isControlling);
                    }
                }
            }

            public IceCandidatePair? FindPair(IIceEndPoint localEndPoint, IPEndPoint remoteEndPoint)
            {
                lock (_syncLock)
                {
                    return _pairs.FirstOrDefault(p =>
                        p.LocalCandidate.IceEndPoint == localEndPoint &&
                        p.RemoteCandidate.EndPoint.IsEquivalent(remoteEndPoint));
                }
            }
        }
    }
}
