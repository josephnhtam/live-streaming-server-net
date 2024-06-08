using Azure.Storage.Blobs;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Configurations;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Internal.Logging;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Internal
{
    internal class HlsAzureBlobStorageAdapter : IHlsStorageAdapter
    {
        private readonly BlobContainerClient _containerClient;
        private readonly HlsAzureBlobStorageConfiguration _config;
        private readonly ILogger _logger;

        public HlsAzureBlobStorageAdapter(
            BlobContainerClient containerClient,
            HlsAzureBlobStorageConfiguration config,
            ILogger<HlsAzureBlobStorageAdapter> logger)
        {
            _containerClient = containerClient;
            _config = config;
            _logger = logger;
        }

        public async Task<StoringResult> StoreAsync(
            StreamProcessingContext context,
            IReadOnlyList<Manifest> manifests,
            IReadOnlyList<ManifestTsSegment> tsSegments,
            CancellationToken cancellationToken)
        {
            var storedTsSegments = await UploadTsSegmentsAsync(context, tsSegments, cancellationToken);
            var storedManifestFiles = await UploadManifestFilesAsync(context, manifests, cancellationToken);
            return new StoringResult(storedManifestFiles, storedTsSegments);
        }

        private async Task<IReadOnlyList<StoredTsSegment>> UploadTsSegmentsAsync(
            StreamProcessingContext context,
            IReadOnlyList<ManifestTsSegment> tsSegments,
            CancellationToken cancellationToken)
        {
            var dirPath = Path.GetDirectoryName(context.OutputPath) ?? string.Empty;

            var tasks = new List<Task<StoredTsSegment>>();

            foreach (var tsSegment in tsSegments)
            {
                var tsSegmentPath = Path.Combine(dirPath, tsSegment.FileName);
                tasks.Add(UploadTsSegmentAsync(tsSegment.FileName, tsSegmentPath, cancellationToken));
            }

            return await Task.WhenAll(tasks);

            async Task<StoredTsSegment> UploadTsSegmentAsync
                (string tsSegmentName, string tsSegmentPath, CancellationToken cancellationToken)
            {
                try
                {
                    var blobPath = _config.BlobPathResolver.ResolveBlobPath(context, tsSegmentName);
                    var blobClient = _containerClient.GetBlobClient(blobPath);

                    var response = await blobClient.UploadAsync(tsSegmentPath, _config.TsSegmentsUploadOptions, cancellationToken);
                    return new StoredTsSegment(tsSegmentName, blobClient.Uri);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.UploadingTsSegmentError(
                        context.Processor, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, tsSegmentPath, ex);

                    return new StoredTsSegment(tsSegmentName, null);
                }
            }
        }

        private async Task<IReadOnlyList<StoredManifest>> UploadManifestFilesAsync(
           StreamProcessingContext context,
           IReadOnlyList<Manifest> manifests,
           CancellationToken cancellationToken)
        {
            var tasks = new List<Task<StoredManifest>>();

            foreach (var manifest in manifests)
            {
                tasks.Add(UploadManifestAsync(manifest.Name, manifest.Content, cancellationToken));
            }

            return await Task.WhenAll(tasks);

            async Task<StoredManifest> UploadManifestAsync
                (string name, string content, CancellationToken cancellationToken)
            {
                try
                {
                    var blobPath = _config.BlobPathResolver.ResolveBlobPath(context, name);
                    var blobClient = _containerClient.GetBlobClient(blobPath);

                    await blobClient.UploadAsync(new BinaryData(content), _config.ManifestsUploadOptions, cancellationToken);
                    return new StoredManifest(name, blobClient.Uri);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.UploadingManifestFileError(
                        context.Processor, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, name, ex);

                    return new StoredManifest(name, null);
                }
            }
        }

        public async Task DeleteAsync(StreamProcessingContext context, IReadOnlyList<ManifestTsSegment> tsSegments, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            foreach (var tsSegment in tsSegments)
            {
                tasks.Add(DeleteTsSegmentAsync(tsSegment.FileName, cancellationToken));
            }

            await Task.WhenAll(tasks);

            async Task DeleteTsSegmentAsync
                (string tsSegmentName, CancellationToken cancellationToken)
            {
                try
                {
                    var blobPath = _config.BlobPathResolver.ResolveBlobPath(context, tsSegmentName);
                    var blobClient = _containerClient.GetBlobClient(blobPath);

                    await blobClient.DeleteAsync(cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.DeletingTsSegmentError(
                        context.Processor, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, ex);
                }
            }
        }
    }
}
