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

                    var objectPath = _config.ObjectPathResolver.ResolveObjectPath(context, segmentName);

                    var request = new PutObjectRequest
                    {
                        BucketName = _bucket,
                        Key = objectPath,
                        FilePath = segmentPath
                    };

                    var response = await _s3Client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);

                    return new StoredSegment(segmentName, _config.ObjectUriResolver.ResolveObjectUri(_bucket, objectPath));
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

                    var objectPath = _config.ObjectPathResolver.ResolveObjectPath(context, name);

                    var request = new PutObjectRequest
                    {
                        BucketName = _bucket,
                        Key = objectPath,
                        InputStream = contentStream
                    };

                    var response = await _s3Client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);

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

                    await _s3Client.DeleteObjectAsync(_bucket, objectPath, cancellationToken: cancellationToken).ConfigureAwait(false);
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
