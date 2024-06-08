using Google.Cloud.Storage.V1;
using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Configurations
{
    public class HlsGoogleCloudStorageConfiguration
    {
        public UploadObjectOptions ManifestsUploadObjectOptions { get; set; } = new UploadObjectOptions();
        public UploadObjectOptions TsSegmentsUploadObjectOptions { get; set; } = new UploadObjectOptions();
        public string ManifestsCacheControl { get; set; } = "no-cache,no-store,max-age=0";
        public string TsSegmentsCacheControl { get; set; } = string.Empty;
        public IHlsObjectPathResolver ObjectPathResolver { get; set; } = new DefaultHlsObjectPathResolver();
        public IHlsObjectUriResolver ObjectUriResolver { get; set; } = new DefaultHlsObjectUriResolver();
    }
}
