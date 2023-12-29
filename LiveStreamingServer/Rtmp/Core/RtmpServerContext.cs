using LiveStreamingServer.Rtmp.Core.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpServerContext : IRtmpServerContext
    {
        private readonly Dictionary<IRtmpClientPeerContext, string> _publishStreamPaths = new();
        private readonly Dictionary<string, IRtmpClientPeerContext> _publishingClientPeerContexts = new();

        public string? GetPublishStreamPath(IRtmpClientPeerContext peerContext)
        {
            return _publishStreamPaths.GetValueOrDefault(peerContext);
        }

        public IRtmpClientPeerContext? GetPublishingClientPeerContext(string publishStreamPath)
        {
            return _publishingClientPeerContexts.GetValueOrDefault(publishStreamPath);
        }

        public StartPublishingStreamResult StartPublishingStream(string publishStreamPath, IRtmpClientPeerContext peerContext)
        {
            lock (_publishStreamPaths)
            {
                if (_publishStreamPaths.ContainsKey(peerContext))
                    return StartPublishingStreamResult.AlreadyPublishing;

                if (_publishingClientPeerContexts.ContainsKey(publishStreamPath))
                    return StartPublishingStreamResult.AlreadyExists;

                _publishStreamPaths.Add(peerContext, publishStreamPath);
                _publishingClientPeerContexts.Add(publishStreamPath, peerContext);

                return StartPublishingStreamResult.Succeeded;
            }
        }

        public void StopPublishingStream(string publishStreamPath)
        {
            lock (_publishStreamPaths)
            {
                if (!_publishingClientPeerContexts.TryGetValue(publishStreamPath, out var peerContext))
                    return;

                _publishingClientPeerContexts.Remove(publishStreamPath);
                _publishStreamPaths.Remove(peerContext);
            }
        }

        public void RemoveClientPeerContext(IRtmpClientPeerContext peerContext)
        {
            lock (_publishStreamPaths)
            {
                if (!_publishStreamPaths.TryGetValue(peerContext, out var publishStreamPath))
                    return;

                _publishStreamPaths.Remove(peerContext);
                _publishingClientPeerContexts.Remove(publishStreamPath);
            }
        }
    }

    public enum StartPublishingStreamResult
    {
        Succeeded,
        AlreadyExists,
        AlreadyPublishing,
    }
}
