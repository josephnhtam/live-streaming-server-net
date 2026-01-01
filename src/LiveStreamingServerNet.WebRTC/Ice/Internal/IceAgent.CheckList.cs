namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal partial class IceAgent
    {
        private class CheckList
        {
            private readonly IceAgent _agent;
            private readonly int _maxCheckListSize;
            private readonly object _syncLock;

            private readonly List<LocalIceCandidate> _localCandidates;
            private readonly List<RemoteIceCandidate> _remoteCandidates;
            private readonly List<IceCandidatePair> _pairs;

            private bool _localGatheringComplete;
            private bool _remoteGatheringComplete;

            public CheckList(IceAgent agent)
            {
                _agent = agent;
                _maxCheckListSize = agent._config.MaxCheckListSize;
                _syncLock = agent._syncLock;

                _localCandidates = new List<LocalIceCandidate>();
                _remoteCandidates = new List<RemoteIceCandidate>();
                _pairs = new List<IceCandidatePair>();
            }

            public bool AddLocalCandidate(LocalIceCandidate? candidate)
            {
                lock (_syncLock)
                {
                    if (_localGatheringComplete)
                    {
                        return false;
                    }

                    if (candidate == null)
                    {
                        _localGatheringComplete = true;
                        CheckCompletion();
                        return false;
                    }

                    DoAddLocalCandidate(candidate);
                    return true;
                }

                void DoAddLocalCandidate(LocalIceCandidate localCandidate)
                {
                    var pairAdded = false;
                    var isControlling = _agent.Role == IceRole.Controlling;

                    _localCandidates.Add(localCandidate);
                    foreach (var remoteCandidate in _remoteCandidates)
                    {
                        if (!IceLogic.CanPairCandidates(localCandidate, remoteCandidate))
                            continue;

                        var pair = new IceCandidatePair(localCandidate, remoteCandidate, isControlling);
                        _pairs.Add(pair);
                        pairAdded = true;
                    }

                    if (pairAdded)
                    {
                        RefreshPairs();
                    }
                }
            }

            public bool AddRemoteCandidate(RemoteIceCandidate? candidate)
            {
                lock (_syncLock)
                {
                    if (_remoteGatheringComplete)
                    {
                        return false;
                    }

                    if (candidate == null)
                    {
                        _remoteGatheringComplete = true;
                        CheckCompletion();
                        return false;
                    }

                    DoAddRemoteCandidate(candidate);
                    return true;
                }

                void DoAddRemoteCandidate(RemoteIceCandidate remoteCandidate)
                {
                    var pairAdded = false;
                    var isControlling = _agent.Role == IceRole.Controlling;

                    _remoteCandidates.Add(remoteCandidate);
                    foreach (var localCandidate in _localCandidates)
                    {
                        if (!IceLogic.CanPairCandidates(localCandidate, remoteCandidate))
                            continue;

                        var pair = new IceCandidatePair(localCandidate, remoteCandidate, isControlling);
                        _pairs.Add(pair);
                        pairAdded = true;
                    }

                    if (pairAdded)
                    {
                        RefreshPairs();
                    }
                }
            }

            private void RefreshPairs()
            {
                lock (_syncLock)
                {
                    PrunePairs();
                    UnfreezePairs();
                }

                return;

                void PrunePairs()
                {
                    var toRemoveCount = _pairs.Count - _maxCheckListSize;

                    if (toRemoveCount > 0)
                        return;

                    var removables = _pairs
                        .Where(p => p.State is IceCandidatePairState.Frozen or IceCandidatePairState.Waiting)
                        .OrderBy(p => p.Priority)
                        .Take(toRemoveCount);

                    foreach (var toRemove in removables)
                    {
                        _pairs.Remove(toRemove);
                    }
                }

                void UnfreezePairs()
                {
                    var firstPairByFoundation = _pairs
                        .GroupBy(p => p.Foundation)
                        .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Priority).First());

                    foreach (var pair in firstPairByFoundation.Values)
                    {
                        if (pair.State == IceCandidatePairState.Frozen)
                        {
                            pair.State = IceCandidatePairState.Waiting;
                        }
                    }
                }
            }

            private void CheckCompletion()
            {
                lock (_syncLock)
                {
                    if (!_localGatheringComplete || !_remoteGatheringComplete)
                        return;

                    var allPairsChecked = _pairs.All(p => p.State is IceCandidatePairState.Succeeded or IceCandidatePairState.Failed);

                    // _agent.TryTransitionTo(IceConnectionState.Completed);
                    // _agent.TryTransitionTo(IceConnectionState.Failed);
                }
            }
        }
    }
}
