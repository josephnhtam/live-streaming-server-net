namespace LiveStreamingServerNet.KubernetesPod.Redis.Internal
{
    internal class StreamInfo
    {
        public uint ClientId { get; }
        public string PodNamespace { get; }
        public string PodName { get; }
        public string LocalEndPoint { get; }
        public string RemoteEndPoint { get; }
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }

        public StreamInfo(
            uint clientId,
            string localEndPoint,
            string remoteEndPoint,
            string podNamespace,
            string podName,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            ClientId = clientId;
            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
            PodNamespace = podNamespace;
            PodName = podName;
            StreamPath = streamPath;
            StreamArguments = new Dictionary<string, string>(streamArguments);
        }
    }
}
