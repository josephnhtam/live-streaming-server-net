using Google.Cloud.Storage.V1;
using LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Contracts;

namespace LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Configurations
{
    public class HlsGoogleCloudStorageConfiguration
    {
        public UploadObjectOptions ManifestsUploadObjectOptions { get; set; } = new UploadObjectOptions();
        public UploadObjectOptions TsFilesUploadObjectOptions { get; set; } = new UploadObjectOptions();
        public string ManifestsCacheControl { get; set; } = "no-cache,no-store,max-age=0";
        public string TsFilesCacheControl { get; set; } = string.Empty;
        public IHlsObjectPathResolver ObjectPathResolver { get; set; } = new DefaultHlsObjectPathResolver();
    }
}
