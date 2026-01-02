using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Utilities;
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
                    var isControlling = _agent.Role == IceRole.Controlling;

                    _localCandidates.Add(localCandidate);
                    foreach (var remoteCandidate in _remoteCandidates)
                    {
                        if (!IceLogic.CanPairCandidates(localCandidate, remoteCandidate))
                            continue;

                        var pair = new IceCandidatePair(localCandidate, remoteCandidate, isControlling);
                        _pairs.Add(pair);
                    }

                    PrunePairs();
                    return true;
                }
            }

            public bool AddRemoteCandidate(RemoteIceCandidate remoteCandidate, bool isTriggered)
            {
                lock (_syncLock)
                {
                    var isControlling = _agent.Role == IceRole.Controlling;

                    _remoteCandidates.Add(remoteCandidate);
                    foreach (var localCandidate in _localCandidates)
                    {
                        if (!IceLogic.CanPairCandidates(localCandidate, remoteCandidate))
                            continue;

                        var pair = new IceCandidatePair(localCandidate, remoteCandidate, isControlling);
                        _pairs.Add(pair);

                        if (isTriggered)
                        {
                            TriggerCheck(pair);
                        }
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
                        .Where(p => p.State is IceCandidatePairState.Failed or IceCandidatePairState.Frozen or IceCandidatePairState.Waiting)
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
                    while (_triggeredChecks.TryDequeue(out var pair))
                    {
                        pair.IsTriggered = false;

                        if (pair.State is IceCandidatePairState.InProgress)
                            continue;

                        if (pair.State is IceCandidatePairState.Failed)
                            pair.State = IceCandidatePairState.Waiting;

                        return pair;
                    }

                    UnfreezePairs();

                    return _pairs.Where(p => p.State == IceCandidatePairState.Waiting).OrderByDescending(p => p.Priority).FirstOrDefault();
                }

                void UnfreezePairs()
                {
                    lock (_syncLock)
                    {
                        if (!_pairs.Any(p => p.State is IceCandidatePairState.Waiting or IceCandidatePairState.InProgress))
                        {
                            var nextFrozen = _pairs
                                .Where(p => p.State == IceCandidatePairState.Frozen)
                                .OrderByDescending(p => p.Priority)
                                .FirstOrDefault();

                            if (nextFrozen != null)
                            {
                                nextFrozen.State = IceCandidatePairState.Waiting;
                            }
                        }
                    }
                }
            }

            public void TriggerCheck(IceCandidatePair pair)
            {
                lock (_syncLock)
                {
                    if (pair.IsTriggered || pair.State == IceCandidatePairState.InProgress)
                        return;

                    if (pair.State == IceCandidatePairState.Frozen)
                        pair.State = IceCandidatePairState.Waiting;

                    if (pair.State == IceCandidatePairState.Failed)
                        pair.State = IceCandidatePairState.Waiting;

                    pair.IsTriggered = true;
                    _triggeredChecks.Enqueue(pair);
                }
            }

            public bool AllPairsChecked()
            {
                lock (_syncLock)
                {
                    return _pairs.All(p => p.State is IceCandidatePairState.Succeeded or IceCandidatePairState.Failed);
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
                        if (pair.NominationState is IceCandidateNominationState.ControllingNominating or IceCandidateNominationState.ControlledNominating)
                        {
                            pair.NominationState = IceCandidateNominationState.None;
                        }

                        if (isControlling)
                        {
                            pair.UseCandidateReceived = false;
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
