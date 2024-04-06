namespace LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Contracts
{
    public interface IHlsObjectUriResolver
    {
        Uri ResolveObjectUri(Google.Apis.Storage.v1.Data.Object @object);
    }
}
