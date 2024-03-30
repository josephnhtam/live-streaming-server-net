namespace LiveStreamingServerNet.KubernetesPod.Redis.Internal.Services.Contracts
{
    internal interface IStreamKeyProvider
    {
        string ResolveStreamKey(string streamPath);
    }
}
