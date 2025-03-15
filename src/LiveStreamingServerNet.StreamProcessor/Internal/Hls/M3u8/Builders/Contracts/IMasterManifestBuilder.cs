namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders.Contracts
{
    internal interface IMasterManifestBuilder : IManifestBuilder
    {
        IMasterManifestBuilder AddVariantStream(VariantStream variant);
        IMasterManifestBuilder AddAlternateMedia(AlternateMedia media);
    }
}
