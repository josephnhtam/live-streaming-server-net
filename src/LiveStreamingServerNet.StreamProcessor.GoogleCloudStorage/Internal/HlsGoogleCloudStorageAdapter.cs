using Google.Cloud.Storage.V1;
using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Configurations;
using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Internal.Logging;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Internal
{
    internal class HlsGoogleCloudStorageAdapter : IHlsStorageAdapter
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucket;
        private readonly HlsGoogleCloudStorageConfiguration _config;
        private readonly ILogger _logger;

        public HlsGoogleCloudStorageAdapter(
            StorageClient storageClient,
            string bucket,
            HlsGoogleCloudStorageConfiguration config,
            ILogger<HlsGoogleCloudStorageAdapter> logger)
        {
            _storageClient = storageClient;
            _bucket = bucket;
            _config = config;
            _logger = logger;
        }

        public async Task<StoringResult> StoreAsync(
            StreamProcessingContext context,
            IReadOnlyList<Manifest> manifests,
            IReadOnlyList<Segment> segments,
            CancellationToken cancellationToken)
        {
            var storedSegments = await UploadSegmentsAsync(context, segments, cancellationToken).ConfigureAwait(false);
            var storedManifestFiles = await UploadManifestFilesAsync(context, manifests, cancellationToken).ConfigureAwait(false);
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

            return await Task.WhenAll(tasks).ConfigureAwait(false);

            async Task<StoredSegment> UploadSegmentAsync
                (string segmentName, string segmentPath, CancellationToken cancellationToken)
            {
                try
                {
                    using var fileStream = File.OpenRead(segmentPath);

                    var @object = new Google.Apis.Storage.v1.Data.Object
                    {
                        Bucket = _bucket,
                        Name = _config.ObjectPathResolver.ResolveObjectPath(context, segmentName),
                        CacheControl = _config.SegmentsCacheControl
                    };

                    var result = await _storageClient.UploadObjectAsync(
                        @object, fileStream,
                        options: _config.SegmentsUploadObjectOptions,
                        cancellationToken: cancellationToken).ConfigureAwait(false);

                    return new StoredSegment(segmentName, _config.ObjectUriResolver.ResolveObjectUri(result));
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

            return await Task.WhenAll(tasks).ConfigureAwait(false);

            async Task<StoredManifest> UploadManifestAsync
                (string name, string content, CancellationToken cancellationToken)
            {
                try
                {
                    using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

                    var @object = new Google.Apis.Storage.v1.Data.Object
                    {
                        Bucket = _bucket,
                        Name = _config.ObjectPathResolver.ResolveObjectPath(context, name),
                        CacheControl = _config.ManifestsCacheControl
                    };

                    var result = await _storageClient.UploadObjectAsync(
                        @object, contentStream,
                        options: _config.ManifestsUploadObjectOptions,
                        cancellationToken: cancellationToken).ConfigureAwait(false);

                    return new StoredManifest(name, _config.ObjectUriResolver.ResolveObjectUri(result));
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

            await Task.WhenAll(tasks).ConfigureAwait(false);

            async Task DeleteSegmentAsync
                (string segmentName, CancellationToken cancellationToken)
            {
                try
                {
                    var objectPath = _config.ObjectPathResolver.ResolveObjectPath(context, segmentName);

                    await _storageClient.DeleteObjectAsync(_bucket, objectPath, cancellationToken: cancellationToken).ConfigureAwait(false);
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
