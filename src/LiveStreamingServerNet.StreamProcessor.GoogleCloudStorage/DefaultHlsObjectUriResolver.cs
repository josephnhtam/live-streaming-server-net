using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage
{
    public class DefaultHlsObjectUriResolver : IHlsObjectUriResolver
    {
        public Uri? ResolveObjectUri(Google.Apis.Storage.v1.Data.Object @object)
        {
            return new Uri($"https://storage.googleapis.com/{@object.Bucket}/{@object.Name}");
        }
    }
}
