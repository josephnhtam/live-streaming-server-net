using Amazon.S3;
using Amazon.S3.Model;
using LiveStreamingServerNet.StreamProcessor.AmazonS3.Configurations;
using LiveStreamingServerNet.StreamProcessor.AmazonS3.Internal.Logging;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LiveStreamingServerNet.StreamProcessor.AmazonS3.Internal
{
    internal class HlsAmazonS3Adapter : IHlsStorageAdapter
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucket;
        private readonly HlsAmazonS3Configuration _config;
        private readonly ILogger _logger;

        public HlsAmazonS3Adapter(
            IAmazonS3 s3Client,
            string bucket,
            HlsAmazonS3Configuration config,
            ILogger<HlsAmazonS3Adapter> logger)
        {
            _s3Client = s3Client;
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

                    var objectPath = _config.ObjectPathResolver.ResolveObjectPath(context, tsSegmentName);

                    var request = new PutObjectRequest
                    {
                        BucketName = _bucket,
                        Key = objectPath,
                        FilePath = tsSegmentPath
                    };

                    var response = await _s3Client.PutObjectAsync(request, cancellationToken);

                    return new StoredTsSegment(tsSegmentName, _config.ObjectUriResolver.ResolveObjectUri(_bucket, objectPath));
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

                    var objectPath = _config.ObjectPathResolver.ResolveObjectPath(context, name);

                    var request = new PutObjectRequest
                    {
                        BucketName = _bucket,
                        Key = objectPath,
                        InputStream = contentStream
                    };

                    var response = await _s3Client.PutObjectAsync(request, cancellationToken);

                    return new StoredManifest(name, _config.ObjectUriResolver.ResolveObjectUri(_bucket, objectPath));
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

                    await _s3Client.DeleteObjectAsync(_bucket, objectPath, cancellationToken: cancellationToken);
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
