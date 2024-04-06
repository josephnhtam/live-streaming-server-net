using Google.Cloud.Storage.V1;

namespace LiveStreamingServerNet.Transmuxer.GoogleCloudStorage
{
    public record struct HlsUploadObjectOptions(UploadObjectOptions Options, string CacheControl);
}
