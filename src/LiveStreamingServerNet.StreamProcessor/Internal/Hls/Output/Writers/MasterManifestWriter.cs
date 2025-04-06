using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers
{
    internal class MasterManifestWriter : IMasterManifestWriter
    {
        public async Task WriteAsync(
            string manifestOutputPath,
            IEnumerable<VariantStream> variantStreams,
            IEnumerable<AlternateMedia> alternateMedia,
            CancellationToken cancellationToken = default)
        {
            var manifestDirPath = Path.GetDirectoryName(manifestOutputPath) ?? string.Empty;

            var manifestBuilder = new MasterManifestBuilder();

            foreach (var variantStream in variantStreams)
            {
                var relativePath = PathHelper.GetRelativePath(variantStream.Uri, manifestDirPath);
                manifestBuilder.AddVariantStream(variantStream with { Uri = relativePath });
            }

            foreach (var alternate in alternateMedia)
            {
                var relativePath = PathHelper.GetRelativePath(alternate.Uri, manifestDirPath);
                manifestBuilder.AddAlternateMedia(alternate with { Uri = relativePath });
            }

            var manifest = manifestBuilder.Build();

            await FileHelper.WriteToFileAsync(manifestOutputPath, manifest);
        }
    }
}
