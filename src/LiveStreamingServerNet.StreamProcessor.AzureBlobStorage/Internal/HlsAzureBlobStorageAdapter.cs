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
            IReadOnlyList<Segment> segments,
            CancellationToken cancellationToken)
        {
            var storedSegments = await UploadSegmentsAsync(context, segments, cancellationToken);
            var storedManifestFiles = await UploadManifestFilesAsync(context, manifests, cancellationToken);
            return new StoringResult(storedManifestFiles, storedSegments);
        }

        private async Task<IReadOnlyList<StoredSegment>> UploadSegmentsAsync(
            StreamProcessingContext context,
            IReadOnlyList<Segment> segments,
            CancellationToken cancellationToken)
        {
            var dirPath = Path.GetDirectoryName(context.OutputPath) ?? string.Empty;

            var tasks = new List<Task<StoredSegment>>();

            foreach (var segment in segments)
            {
                var segmentPath = Path.Combine(dirPath, segment.FileName);
                tasks.Add(UploadSegmentAsync(segment.FileName, segmentPath, cancellationToken));
            }

            return await Task.WhenAll(tasks);

            async Task<StoredSegment> UploadSegmentAsync
                (string segmentName, string segmentPath, CancellationToken cancellationToken)
            {
                try
                {
                    var blobPath = _config.BlobPathResolver.ResolveBlobPath(context, segmentName);
                    var blobClient = _containerClient.GetBlobClient(blobPath);

                    var response = await blobClient.UploadAsync(segmentPath, _config.SegmentsUploadOptions, cancellationToken);
                    return new StoredSegment(segmentName, blobClient.Uri);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.UploadingSegmentError(
                        context.Processor, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, segmentPath, ex);

                    return new StoredSegment(segmentName, null);
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

        public async Task DeleteAsync(StreamProcessingContext context, IReadOnlyList<Segment> segments, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            foreach (var segment in segments)
            {
                tasks.Add(DeleteSegmentAsync(segment.FileName, cancellationToken));
            }

            await Task.WhenAll(tasks);

            async Task DeleteSegmentAsync
                (string segmentName, CancellationToken cancellationToken)
            {
                try
                {
                    var blobPath = _config.BlobPathResolver.ResolveBlobPath(context, segmentName);
                    var blobClient = _containerClient.GetBlobClient(blobPath);

                    await blobClient.DeleteAsync(cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.DeletingSegmentError(
                        context.Processor, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, ex);
                }
            }
        }
    }
}
