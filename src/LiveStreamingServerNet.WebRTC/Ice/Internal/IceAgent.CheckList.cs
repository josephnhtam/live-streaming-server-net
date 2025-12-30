namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal partial class IceAgent
    {
        private class CheckList
        {
            private readonly int _maxCheckListSize;
            private readonly object _syncLock;

            private List<IceCandidatePair> _pairs;

            public CheckList(IceAgent agent)
            {
                _maxCheckListSize = agent._config.MaxCheckListSize;
                _syncLock = agent._syncLock;

                _pairs = new List<IceCandidatePair>();
            }


        }
    }
}
