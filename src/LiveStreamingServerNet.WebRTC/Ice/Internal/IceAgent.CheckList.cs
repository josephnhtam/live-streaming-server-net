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

            public bool AddRemoteCandidate(RemoteIceCandidate remoteCandidate)
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
                    UnfreezePairs();

                    var triggeredPair = _pairs
                        .FirstOrDefault(p => p is { IsTriggered: true, State: IceCandidatePairState.Waiting or IceCandidatePairState.Frozen });

                    if (triggeredPair != null)
                    {
                        triggeredPair.IsTriggered = false;
                        return triggeredPair;
                    }

                    return _pairs.OrderByDescending(p => p.Priority).FirstOrDefault();
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

            public bool AllPairsChecked()
            {
                lock (_syncLock)
                {
                    return _pairs.All(p => p.State is IceCandidatePairState.Succeeded or IceCandidatePairState.Failed);
                }
            }
        }
    }
}
