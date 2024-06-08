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
                    using var fileStream = File.OpenRead(tsSegmentPath);

                    var @object = new Google.Apis.Storage.v1.Data.Object
                    {
                        Bucket = _bucket,
                        Name = _config.ObjectPathResolver.ResolveObjectPath(context, tsSegmentName),
                        CacheControl = _config.TsSegmentsCacheControl
                    };

                    var result = await _storageClient.UploadObjectAsync(
                        @object, fileStream,
                        options: _config.TsSegmentsUploadObjectOptions,
                        cancellationToken: cancellationToken);

                    return new StoredTsSegment(tsSegmentName, _config.ObjectUriResolver.ResolveObjectUri(result));
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
                        cancellationToken: cancellationToken);

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
                    var objectPath = _config.ObjectPathResolver.ResolveObjectPath(context, tsSegmentName);

                    await _storageClient.DeleteObjectAsync(_bucket, objectPath, cancellationToken: cancellationToken);
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
