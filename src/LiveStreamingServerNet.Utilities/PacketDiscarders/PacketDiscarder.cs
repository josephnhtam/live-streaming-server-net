using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;

namespace LiveStreamingServerNet.Utilities.PacketDiscarders
{
    public class PacketDiscarder : IPacketDiscarder
    {
        private readonly PacketDiscarderConfiguration _config;
        private readonly OnDiscardBegin? _onDiscardBegin;
        private readonly OnDiscardEnd? _onDiscardEnd;

        private bool _isDiscarding;

        public PacketDiscarder(
            PacketDiscarderConfiguration config, OnDiscardBegin? onDiscardBegin = null, OnDiscardEnd? onDiscardEnd = null)
        {
            _config = config;
            _onDiscardBegin = onDiscardBegin;
            _onDiscardEnd = onDiscardEnd;
        }

        public bool ShouldDiscardPacket(bool isDiscardable, long outstandingSize, long outstandingCount)
        {
            if (!isDiscardable)
            {
                return false;
            }

            if (_isDiscarding)
            {
                if (outstandingSize <= _config.TargetOutstandingPacketsSize &&
                    outstandingCount <= _config.TargetOutstandingPacketsCount)
                {
                    _onDiscardEnd?.Invoke(outstandingSize, outstandingCount);
                    _isDiscarding = false;
                    return false;
                }

                return true;
            }

            if (outstandingSize > _config.MaxOutstandingPacketsSize ||
                outstandingCount > _config.MaxOutstandingPacketsCount)
            {
                _onDiscardBegin?.Invoke(outstandingSize, outstandingCount);
                _isDiscarding = true;
                return true;
            }

            return false;
        }
    }

    public delegate void OnDiscardBegin(long outstandingSize, long outstandingCount);
    public delegate void OnDiscardEnd(long outstandingSize, long outstandingCount);
}
