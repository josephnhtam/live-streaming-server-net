using LiveStreamingServerNet.Rtmp.Internal.Filtering.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Filtering
{
    internal class Filter<TItem> : IFilter<TItem> where TItem : struct
    {
        private readonly HashSet<TItem> _include;
        private readonly HashSet<TItem> _exclude;

        public Filter(ISet<TItem> include, ISet<TItem> exclude)
        {
            _include = new HashSet<TItem>(include);
            _exclude = new HashSet<TItem>(exclude);
        }

        public bool IsAllowed(TItem codec)
        {
            if (_include.Count > 0 && !_include.Contains(codec))
                return false;

            if (_exclude.Count > 0 && _exclude.Contains(codec))
                return false;

            return true;
        }
    }
}
